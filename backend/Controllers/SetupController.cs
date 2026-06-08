using System.Runtime.InteropServices;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        [Authorize(Roles = nameof(Role.Admin) + "," + nameof(Role.SuperAdmin))]
        [HttpGet("setup/{companyId}")]
        public async Task<IActionResult> GetSetupData(int companyId)
        {
            var isSuperAdmin = User.IsInRole(nameof(Role.SuperAdmin));
            if (!isSuperAdmin)
            {
                var companyIdFromToken = GetCompanyIdFromToken();
                if (companyIdFromToken == null)
                {
                    return Unauthorized("Empresa no encontrada en el token.");
                }

                if (companyIdFromToken != companyId)
                {
                    return Forbid("El usuario no tiene permiso para ver esta empresa.");
                }
            }

            var company = await _context.Companies
                .Include(c => c.ChatbotSettings)
                .Include(c => c.Faqs)
                .Include(c => c.DocumentSources)

                .FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            var setupResponse = new SetupResponseDto
            {
                CompanySetup = new CompanySetupDto
                {
                    CompanyName = company.Name,
                    LegalName = company.LegalName,
                    Cif = company.CIF,
                    Email = company.Email,
                    Sector = company.Sector,
                    Website = company.Website,
                    Description = company.Description
                },
                PersonalitySetup = company.ChatbotSettings != null ? new PersonalitySetupDto
                {
                    ChatbotName = company.ChatbotSettings.ChatbotName,
                    ChatbotTone = company.ChatbotSettings.ChatbotTone,
                    GreetingMessage = company.ChatbotSettings.GreetingMessage,
                    FallbackMessage = company.ChatbotSettings.FallbackMessage
                } : new PersonalitySetupDto(),
                KnowledgeSetup = new KnowledgeSetupDto
                {
                    Faqs = company.Faqs.Select(f => new FaqResponseDto
                    {
                        Id = f.Id,
                        Question = f.Question,
                        Answer = f.Answer,
                        CreatedAt = f.CreatedAt,
                        UpdatedAt = f.UpdatedAt
                    }).ToList(),
                    Documents = company.DocumentSources.Select(d => new DocumentSourceResponseDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        CreatedAt = d.CreatedAt
                    }).ToList()
                }
            };

            return Ok(setupResponse);
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
                    FallbackMessage = setupData.PersonalitySetup.FallbackMessage,
                    PrimaryColor = setupData.AppearanceSetup.PrimaryColor,
                    ShowAvatar = setupData.AppearanceSetup.ShowChatbotAvatar,
                    WidgetPosition = setupData.AppearanceSetup.WidgetPosition
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

        // Endpoint para actualizar solo los datos de la empresa
        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("company")]
        public async Task<IActionResult> SaveCompanySetup(CompanySetupDto companySetup)
        {
            var userId = GetUserIdFromToken();
            var companyId = GetCompanyIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            else if (companyId == null || user.CompanyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }
            else if (companyId != null && user.CompanyId != companyId)
            {
                return Forbid("El usuario no tiene permiso para editar esta empresa.");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            try
            {
                company.Name = companySetup.CompanyName;
                company.LegalName = companySetup.LegalName;
                company.CIF = companySetup.Cif;
                company.Email = companySetup.Email;
                company.Sector = companySetup.Sector;
                company.Website = companySetup.Website;
                company.Description = companySetup.Description;
                _context.Companies.Update(company);
                await _context.SaveChangesAsync();

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
                return StatusCode(500, $"Error al guardar los datos de la empresa: {ex.Message}");
            }
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet("company/subscription")]
        public async Task<IActionResult> GetCompanySubscription()
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            return Ok(new { subscriptionId = company.SubscriptionId });
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPatch("company/subscription/{subscriptionId}")]
        public async Task<IActionResult> UpdateCompanySubscription(int subscriptionId)
        {
            var userId = GetUserIdFromToken();
            var companyId = GetCompanyIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            else if (companyId == null || user.CompanyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }
            else if (companyId != null && user.CompanyId != companyId)
            {
                return Forbid("El usuario no tiene permiso para editar esta empresa.");
            }

            var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (subscription == null)
            {
                return NotFound("Plan no encontrado.");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            company.SubscriptionId = subscriptionId;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return Ok(new { subscriptionId = company.SubscriptionId, message = "Plan actualizado correctamente." });
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("personalization")]
        public async Task<IActionResult> SaveCompanyPersonalization(PersonalitySetupDto personalitySetup)
        {
            var userId = GetUserIdFromToken();
            var companyId = GetCompanyIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            else if (companyId == null || user.CompanyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }
            else if (companyId != null && user.CompanyId != companyId)
            {
                return Forbid("El usuario no tiene permiso para editar esta empresa.");
            }

            var company = await _context.Companies
                .Include(c => c.ChatbotSettings)
                .FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            var chatbotSettings = company.ChatbotSettings;
            chatbotSettings.ChatbotName = personalitySetup.ChatbotName;
            chatbotSettings.ChatbotTone = personalitySetup.ChatbotTone;
            chatbotSettings.GreetingMessage = personalitySetup.GreetingMessage;
            chatbotSettings.FallbackMessage = personalitySetup.FallbackMessage;

            _context.ChatbotSettings.Update(chatbotSettings);
            await _context.SaveChangesAsync();
            return Ok(personalitySetup);
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("appearance")]
        public async Task<IActionResult> SaveCompanyAppearance(AppearanceSetupDto appearanceSetup)
        {
            var userId = GetUserIdFromToken();
            var companyId = GetCompanyIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            else if (companyId == null || user.CompanyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }
            else if (companyId != null && user.CompanyId != companyId)
            {
                return Forbid("El usuario no tiene permiso para editar esta empresa.");
            }

            var company = await _context.Companies
                .Include(c => c.ChatbotSettings)
                .FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            var chatbotSettings = company.ChatbotSettings;
            chatbotSettings.PrimaryColor = appearanceSetup.PrimaryColor;
            chatbotSettings.ShowAvatar = appearanceSetup.ShowChatbotAvatar;
            chatbotSettings.WidgetPosition = appearanceSetup.WidgetPosition;

            _context.ChatbotSettings.Update(chatbotSettings);
            await _context.SaveChangesAsync();
            return Ok(appearanceSetup);
        }

        [Authorize(Roles = nameof(Role.Admin))]
        [HttpPost("faqs")]
        public async Task<IActionResult> SaveCompanyFaqs(List<FaqDto> knowledgeSetup)
        {
            var userId = GetUserIdFromToken();
            var companyId = GetCompanyIdFromToken();
            if (userId == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized("Usuario no encontrado.");
            }
            else if (companyId == null || user.CompanyId == null)
            {
                return Forbid("El usuario no tiene una empresa asignada.");
            }
            else if (companyId != null && user.CompanyId != companyId)
            {
                return Forbid("El usuario no tiene permiso para editar esta empresa.");
            }

            var company = await _context.Companies
                .Include(c => c.Faqs)
                .FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            _context.Faqs.RemoveRange(company.Faqs);

            foreach (var faqDto in knowledgeSetup)
            {
                var faq = new Faq
                {
                    CompanyId = company.Id,
                    Question = faqDto.Question,
                    Answer = faqDto.Answer,
                    CreatedAt = faqDto.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = faqDto.UpdatedAt ?? DateTime.UtcNow
                };
                _context.Faqs.Add(faq);
            }

            await _context.SaveChangesAsync();
            return Ok(knowledgeSetup);
        }

        [Authorize(Roles = nameof(Role.SuperAdmin))]
        [HttpPatch("company/{companyId}/status")]
        public async Task<IActionResult> UpdateCompanyStatus(int companyId, UpdateCompanyStatusDto updateStatus)
        {
            if (string.IsNullOrWhiteSpace(updateStatus.Status))
            {
                return BadRequest("El estado es obligatorio.");
            }

            if (!Enum.TryParse<CompanyStatus>(updateStatus.Status, true, out var nextStatus))
            {
                return BadRequest("Estado de empresa no valido.");
            }

            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Empresa no encontrada.");
            }

            company.Status = nextStatus;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();

            return Ok(new { companyId = company.Id, status = company.Status.ToString() });
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