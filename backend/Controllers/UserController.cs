using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var users = await _context.Users
                .Where(u => u.CompanyId == companyId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Role = u.Role.ToString(),
                    Status = u.Status.ToString(),
                    u.CreatedAt
                })
                .ToListAsync();

            var userDTOs = users.Select(u => new UserDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Role = u.Role,
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(userDTOs);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO createUserDTO)
        { 
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var user = new User
            {
                UserName = createUserDTO.UserName,
                Email = createUserDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDTO.Password),
                Role = Enum.TryParse<Role>(createUserDTO.Role, out var role) ? role : Role.User,
                Status = UserStatus.Active,
                CompanyId = companyId.Value
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDTO = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, userDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);
            if (user == null) return NotFound();

            user.UserName = updateUserDTO.UserName ?? user.UserName;
            user.Email = updateUserDTO.Email ?? user.Email;
            user.Role = updateUserDTO.Role != null && Enum.TryParse<Role>(updateUserDTO.Role, out var role) ? role : user.Role;
            user.Status = updateUserDTO.Status != null && Enum.TryParse<UserStatus>(updateUserDTO.Status, out var status) ? status : user.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]    
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == companyId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        

        private int? GetCompanyIdFromToken()
        {
            var companyIdClaim = User.FindFirst("companyId");
            if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return companyId;
            }
            return null;
        }
    }
}