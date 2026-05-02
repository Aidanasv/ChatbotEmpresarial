using System.Runtime.InteropServices;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SetupController(AppDbContext context)
        {
            _context = context;
        }


        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("setupInitial")]
        public async Task<IActionResult> SetupInitial(SetupResponseDto setupData)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var subscription = await _context.Subscriptions.ToListAsync();
            var company = new Company
            {
                Name = setupData.CompanySetup.CompanyName,
                LegalName = setupData.CompanySetup.LegalName,
                CIF = setupData.CompanySetup.Cif,
                Email = setupData.CompanySetup.Email,
                Sector = setupData.CompanySetup.Sector,
                Website = setupData.CompanySetup.Website,
                Description = setupData.CompanySetup.Description,
                SubscriptionId = subscription.FirstOrDefault()?.Id ?? 0,
                ChatbotSettings = new ChatbotSettings
                {
                    ChatbotName = setupData.PersonalitySetup.ChatbotName,
                    ChatbotTone = setupData.PersonalitySetup.ChatbotTone,
                    GreetingMessage = setupData.PersonalitySetup.GreetingMessage,
                    FallbackMessage = setupData.PersonalitySetup.FallbackMessage
                }
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                user.CompanyId = company.Id;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Configuración inicial guardada correctamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al guardar la configuración inicial: {ex.Message}");
            }
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("company")]
        public async Task<IActionResult> SaveCompanySetup(CompanySetupDto companySetup)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }

            var subscription = await _context.Subscriptions.ToListAsync();
            var company = new Company
            {
                Name = companySetup.CompanyName,
                LegalName = companySetup.LegalName,
                CIF = companySetup.Cif,
                Email = companySetup.Email,
                Sector = companySetup.Sector,
                Website = companySetup.Website,
                Description = companySetup.Description,
                SubscriptionId = subscription.FirstOrDefault()?.Id ?? 0
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                user.CompanyId = company.Id;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var companySetupResponseDto = new CompanySetupResponseDto
                {
                    Id = company.Id,
                    CompanyName = company.Name,
                    LegalName = company.LegalName,
                    Cif = company.CIF,
                    Email = company.Email,
                    Sector = company.Sector,
                    Website = company.Website,
                    Description = company.Description,
                    RenewalDate = company.RenewalDate,
                    CreatedAt = company.CreatedAt,
                    Status = company.Status.ToString()
                };

                return Ok(companySetupResponseDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al guardar los datos de la empresa: {ex.Message}");
            }
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("personalization")]
        public async Task<IActionResult> SaveCompanyPersonalization(PersonalitySetupDto personalitySetup)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            company.ChatbotSettings = new ChatbotSettings
            {
                ChatbotName = personalitySetup.ChatbotName,
                ChatbotTone = personalitySetup.ChatbotTone,
                GreetingMessage = personalitySetup.GreetingMessage,
                FallbackMessage = personalitySetup.FallbackMessage
            };
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
            return Ok(personalitySetup);
        }

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}