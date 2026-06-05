using System.Security.Cryptography;
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
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("analytics")]
        public IActionResult GetAnalytics()
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();


            var totalConversations = _context.Conversations.Include(c => c.ChatbotSettings)
                .Where(c => c.ChatbotSettings != null && c.ChatbotSettings.CompanyId == companyId)
                .Count();
            var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);
            var totalActiveUsers = _context.Conversations
            .Include(c => c.ChatbotSettings)
            .Where(c => c.ChatbotSettings != null && c.ChatbotSettings.CompanyId == companyId)
             .Where(c => _context.Messages
             .Where(m => m.ConversationId == c.Id)
             .OrderByDescending(m => m.CreatedAt)
             .Select(m => m.CreatedAt)
             .FirstOrDefault() >= fifteenMinutesAgo)
             .Count();
            var averageMessagesByConversation = _context.Conversations
            .Where(c => c.ChatbotSettings != null && c.ChatbotSettings.CompanyId == companyId)
            .Select(c => (double?)c.Messages.Count)
            .Average() ?? 0.0;
            var averageResponseTime = _context.Messages
            .Where(m => m.Conversation.ChatbotSettings != null && m.Conversation.ChatbotSettings.CompanyId == companyId)
            .Where(m => m.Role == RoleInConversation.User)
            .Select(m => (double?)_context.Messages
                .Where(resp => resp.ConversationId == m.ConversationId && resp.Role == RoleInConversation.Bot && resp.CreatedAt > m.CreatedAt)
                .OrderBy(resp => resp.CreatedAt)
                // Usamos EF.Functions.DateDiffSeconds para que se traduzca correctamente a SQL (ej: DATEDIFF o TIMESTAMPDIFF)
                .Select(resp => EF.Functions.DateDiffSecond(m.CreatedAt, resp.CreatedAt))
                .FirstOrDefault())
            .Average() ?? 0.0;

            var analytics = new AnalyticsDTO
            {
                TotalConversations = totalConversations,
                TotalActiveUsers = totalActiveUsers,
                AverageMessagesByConversation = (int)averageMessagesByConversation,
                AverageResponseTime = (int)averageResponseTime
            };

            return Ok(analytics);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("conversations")]
        public IActionResult GetConversations(int limit = 5, int offset = 0)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var conversations = _context.Conversations
               .Include(c => c.Messages)
               .Where(c => c.ChatbotSettings != null && c.ChatbotSettings.CompanyId == companyId)
                .OrderByDescending(c => c.CreatedAt)
               .Skip(offset)
               .Take(limit)
               .ToList();

            var conversationAnalytics = conversations.Select(c => new ConversationAnalyticsDTO
            {
                ConversationId = c.Id,
                CustomerName = c.CustomerName,
                Role = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Role.ToString()).FirstOrDefault() ?? "",
                LastMessageTime = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt.ToString("g")).FirstOrDefault() ?? "",
                LastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault() ?? "",
                Status = c.Status.ToString()
            }).ToList();
            return Ok(conversationAnalytics);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("conversationsById/{conversationId}")]
        public IActionResult GetConversationById(int conversationId)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();

            var conversation = _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefault(c => c.Id == conversationId && c.ChatbotSettings != null && c.ChatbotSettings.CompanyId == companyId);

            if (conversation == null) return NotFound();

            var conversationDto = new ConversationMensageDto
            {
                Id = conversation.Id,
                ChatbotSettingsId = conversation.ChatbotSettingsId,
                CustomerEmail = conversation.CustomerEmail,
                CustomerName = conversation.CustomerName,
                CustomerPhone = conversation.CustomerPhone,
                Topic = conversation.Topic,
                Status = conversation.Status.ToString(),
                CreatedAt = conversation.CreatedAt,
                Messages = conversation.Messages.Select(m => new MenssageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    Role = m.Role.ToString(),
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };

            return Ok(conversationDto);
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