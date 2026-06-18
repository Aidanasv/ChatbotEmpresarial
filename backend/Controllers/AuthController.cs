using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthController(AppDbContext context, IConfiguration configuration, IEmailSender emailSender)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("El correo ya está registrado.");
            }

            var newUser = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Admin
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            string token = CreateToken(newUser);

            return Ok(new
            {
                message = "Registro exitoso. Pendiente de completar datos de empresa.",
                token = token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .ThenInclude(c => c!.ChatbotSettings)
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("Correo o contraseña incorrectos.");
            }

            string token = CreateToken(user);

            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                token = token
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Ok(new { message = "Si el correo existe, enviaremos instrucciones para restablecer la contraseña." });
            }

            var rawToken = GenerateResetToken();
            user.PasswordResetTokenHash = ComputeSha256(rawToken);
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
            user.PasswordResetRequestedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var frontendBaseUrl = GetFrontendBaseUrl();
            var resetLink = $"{frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";
            await _emailSender.SendPasswordResetEmailAsync(user.Email, user.UserName, resetLink);

            return Ok(new { message = "Si el correo existe, enviaremos instrucciones para restablecer la contraseña." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || string.IsNullOrWhiteSpace(user.PasswordResetTokenHash) || user.PasswordResetTokenExpiresAt == null)
            {
                return BadRequest("El enlace de recuperación no es válido.");
            }

            if (user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
            {
                return BadRequest("El enlace de recuperación ha expirado.");
            }

            var requestTokenHash = ComputeSha256(request.Token);
            if (!FixedTimeEquals(user.PasswordResetTokenHash, requestTokenHash))
            {
                return BadRequest("El enlace de recuperación no es válido.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            user.PasswordResetRequestedAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("La contraseña actual es incorrecta.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;
            user.PasswordResetRequestedAt = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

        private string CreateToken(User userToken)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("userId", userToken.Id.ToString()),
                        new Claim(ClaimTypes.Role, userToken.Role.ToString()),
                        new Claim(ClaimTypes.Email, userToken.Email),
                        new Claim("companyId", userToken.CompanyId?.ToString() ?? string.Empty),
                        new Claim("chatbotId", userToken.Company?.ChatbotSettings?.Id.ToString() ?? string.Empty)
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        private string GenerateResetToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        }

        private string ComputeSha256(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }

        private bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            return leftBytes.Length == rightBytes.Length && CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private string GetFrontendBaseUrl()
        {
            var configured = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                ?? _configuration["FrontendBaseUrl"]
                ?? "http://localhost:3000";

            return configured.TrimEnd('/');
        }
    }
}