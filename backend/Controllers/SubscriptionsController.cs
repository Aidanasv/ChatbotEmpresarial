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
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubscriptionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = nameof(Role.Admin) + "," + nameof(Role.SuperAdmin))]
        public async Task<IActionResult> GetSubscriptions()
        {
            var subscriptions = await _context.Subscriptions
                .OrderBy(s => s.Price)
                .ToListAsync();

            var companyCounts = await _context.Companies
                .GroupBy(company => company.SubscriptionId)
                .Select(group => new { SubscriptionId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.SubscriptionId, item => item.Count);

            var response = subscriptions.Select(subscription =>
            {
                var companiesCount = companyCounts.TryGetValue(subscription.Id, out var count) ? count : 0;
                return new SubscriptionAdminDto
                {
                    Id = subscription.Id,
                    Name = subscription.Name,
                    Price = subscription.Price,
                    MaxUsers = subscription.MaxUsers,
                    Features = subscription.Features
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList(),
                    CompaniesCount = companiesCount,
                    ProjectedMonthlyRevenue = subscription.Price * companiesCount
                };
            }).ToList();

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = nameof(Role.SuperAdmin))]
        public async Task<IActionResult> CreateSubscription(UpsertSubscriptionDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("El nombre del plan es obligatorio.");
            }

            var subscription = new Subscription
            {
                Name = request.Name.Trim(),
                Price = request.Price,
                MaxUsers = request.MaxUsers,
                Features = string.Join(", ", request.Features.Where(feature => !string.IsNullOrWhiteSpace(feature)).Select(feature => feature.Trim()))
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return Ok(new SubscriptionAdminDto
            {
                Id = subscription.Id,
                Name = subscription.Name,
                Price = subscription.Price,
                MaxUsers = subscription.MaxUsers,
                Features = subscription.Features
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                CompaniesCount = 0,
                ProjectedMonthlyRevenue = 0
            });
        }

        [HttpPut("{subscriptionId}")]
        [Authorize(Roles = nameof(Role.SuperAdmin))]
        public async Task<IActionResult> UpdateSubscription(int subscriptionId, UpsertSubscriptionDto request)
        {
            var subscription = await _context.Subscriptions.FirstOrDefaultAsync(item => item.Id == subscriptionId);
            if (subscription == null)
            {
                return NotFound("Plan no encontrado.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("El nombre del plan es obligatorio.");
            }

            subscription.Name = request.Name.Trim();
            subscription.Price = request.Price;
            subscription.MaxUsers = request.MaxUsers;
            subscription.Features = string.Join(", ", request.Features.Where(feature => !string.IsNullOrWhiteSpace(feature)).Select(feature => feature.Trim()));

            _context.Subscriptions.Update(subscription);
            await _context.SaveChangesAsync();

            var companiesCount = await _context.Companies.CountAsync(company => company.SubscriptionId == subscription.Id);

            return Ok(new SubscriptionAdminDto
            {
                Id = subscription.Id,
                Name = subscription.Name,
                Price = subscription.Price,
                MaxUsers = subscription.MaxUsers,
                Features = subscription.Features
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                CompaniesCount = companiesCount,
                ProjectedMonthlyRevenue = subscription.Price * companiesCount
            });
        }
    }
}
