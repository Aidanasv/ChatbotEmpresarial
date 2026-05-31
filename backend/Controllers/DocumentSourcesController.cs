using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentSourcesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly int _maxFileSize = 10 * 1024 * 1024; // 10 MB

        public DocumentSourcesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UploadDocumentSource(List<IFormFile> files)
        {
            var companyId = GetCompanyIdFromToken();
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            foreach (var file in files)
            {
                if (file.Length == 0) continue;
                if (file.Length > _maxFileSize)
                {
                    return BadRequest($"File {file.FileName} exceeds the maximum allowed size of 10 MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = Enum.GetValues(typeof(TypeOfDocument)).Cast<TypeOfDocument>().Select(e => "." + e.ToString()).ToList();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest($"File {file.FileName} has an invalid file type.");
                }

                try
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedDocuments", companyId.ToString());
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(folderPath, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var documentSource = new DocumentSource
                    {
                        Id = uniqueFileName,
                        CompanyId = companyId.Value,
                        Name = file.FileName,
                        DocumentType = extension switch
                        {
                            ".pdf" => TypeOfDocument.pdf,
                            ".docx" => TypeOfDocument.docx,
                            ".md" => TypeOfDocument.md,
                            ".xlsx" => TypeOfDocument.xlsx,
                            ".txt" => TypeOfDocument.txt,
                            _ => throw new InvalidOperationException("Unsupported file type")
                        },
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.DocumentSources.Add(documentSource);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Ok(new { Messages = "Files uploaded successfully." });
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