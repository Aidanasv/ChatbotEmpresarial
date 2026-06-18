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

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("SuperAdminAnalytics")]
        public IActionResult GetSuperAdminAnalytics()
        {
            var totalConversationsCurrentlyMonth = _context.Conversations
                .Where(c => c.CreatedAt.Month == DateTime.UtcNow.Month && c.CreatedAt.Year == DateTime.UtcNow.Year)
                .Count();

            var MRR = _context.Companies.Include(c => c.Subscription).Sum(c => c.Subscription != null ? c.Subscription.Price : 0);

            var analytics = new AnalyticsSuperAdminDTO
            {
                TotalCompanies = _context.Companies.Count(),
                TotalUsers = _context.Users.Count(),
                TotalConversations = totalConversationsCurrentlyMonth,
                MRR = (double)MRR
            };

            return Ok(analytics);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies([FromQuery] CompaniesQueryDTO query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, 100);
            var search = query.Search?.Trim().ToLower();
            var statusFilter = query.Status?.Trim();

            var baseQuery = _context.Companies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                baseQuery = baseQuery.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Email.ToLower().Contains(search));
            }

            var activeCount = await baseQuery.CountAsync(c => c.Status == CompanyStatus.Active);
            var inReviewCount = await baseQuery.CountAsync(c => c.Status == CompanyStatus.InReview);
            var inactiveCount = await baseQuery.CountAsync(c => c.Status == CompanyStatus.Inactive);

            if (!string.IsNullOrWhiteSpace(statusFilter)
                && !string.Equals(statusFilter, "Todas", StringComparison.OrdinalIgnoreCase)
                && Enum.TryParse<CompanyStatus>(statusFilter, out var parsedStatus))
            {
                baseQuery = baseQuery.Where(c => c.Status == parsedStatus);
            }

            var total = await baseQuery.CountAsync();

            var companiesRaw = await baseQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    SubscriptionName = c.Subscription != null ? c.Subscription.Name : null,
                    SubscriptionPrice = c.Subscription != null ? (decimal?)c.Subscription.Price : null,
                    UsersCount = c.Users.Count,
                    ConversationsCount = c.ChatbotSettings != null ? c.ChatbotSettings.Conversations.Count() : 0,
                    Status = c.Status,
                    c.CreatedAt
                })
                .ToListAsync();

            var companyData = companiesRaw.Select(c => new CompanyAnalyticsDTO
            {
                CompanyId = c.Id,
                Initials = GetInitials(c.Name),
                CompanyName = c.Name,
                CompanyEmail = c.Email,
                CompanySubscription = c.SubscriptionName ?? "N/A",
                Users = c.UsersCount,
                Conversations = c.ConversationsCount,
                MRR = (double?)(c.SubscriptionPrice ?? 0.0m) ?? 0.0,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt.ToString("g")
            }).ToList();

            return Ok(new CompanyAnalyticsPagedResponseDTO
            {
                Items = companyData,
                Total = total,
                Page = page,
                PageSize = pageSize,
                ActiveCount = activeCount,
                InReviewCount = inReviewCount,
                InactiveCount = inactiveCount
            });
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "N/A";

            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = words.Select(w => w[0]).ToArray();

            return new string(initials).ToUpper();
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