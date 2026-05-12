using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaqController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FaqController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize (Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FaqResponseDto>>> GetFaqs()
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();
            var faqs = await _context.Faqs.Where(f => f.CompanyId == companyId).ToListAsync();
            var faqDtos = faqs.Select(f => new FaqResponseDto
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList();
            return Ok(faqDtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FaqResponseDto>> CreateFaq(FaqDto faqDto)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();
            var faq = new Faq
            {
                CompanyId = companyId.Value,
                Question = faqDto.Question,
                Answer = faqDto.Answer,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Faqs.Add(faq);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFaqs), new { companyId = faq.CompanyId }, new FaqResponseDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                CreatedAt = faq.CreatedAt,
                UpdatedAt = faq.UpdatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<FaqResponseDto>> UpdateFaq(int id, FaqDto updatedFaq)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound();

            if (faq.CompanyId != companyId.Value) return Forbid();
            faq.Question = updatedFaq.Question;
            faq.Answer = updatedFaq.Answer;
            faq.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new FaqResponseDto
            {
                Id = faq.Id,
                Question = faq.Question,
                Answer = faq.Answer,
                CreatedAt = faq.CreatedAt,
                UpdatedAt = faq.UpdatedAt
            });

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null) return Unauthorized();
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound();

            if (faq.CompanyId != companyId.Value) return Forbid();
            _context.Faqs.Remove(faq);
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