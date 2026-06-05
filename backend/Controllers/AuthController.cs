using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                .ThenInclude(c => c.ChatbotSettings)
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

        private string CreateToken(User userToken)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("userId", Convert.ToString(userToken.Id)),
                        new Claim(ClaimTypes.Role, userToken.Role.ToString()),
                        new Claim(ClaimTypes.Email, userToken.Email),
                        new Claim("companyId", userToken.CompanyId != null ? userToken.CompanyId.ToString() : ""),
                        new Claim("chatbotId", userToken.Company != null && userToken.Company.ChatbotSettings != null ? userToken.Company.ChatbotSettings.Id.ToString() : "")
                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}