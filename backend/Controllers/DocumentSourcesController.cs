using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GoogleCloud.VertexAI;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using System.Collections.Concurrent;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentSourcesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISubscriptionPermissionService _subscriptionPermissionService;
        private readonly int _maxFileSize = 10 * 1024 * 1024; // 10 MB
        private static readonly ConcurrentDictionary<int, CorpusRebuildStatusResponse> _rebuildStatusByCompany = new();

        public DocumentSourcesController(
            AppDbContext context,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            ISubscriptionPermissionService subscriptionPermissionService)
        {
            _context = context;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _subscriptionPermissionService = subscriptionPermissionService;
        }

        private static VertexAiRagClient CreateRagClient()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<VertexAiRagClient>();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<VertexAiRagClient>();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("corpora")]
        public async Task<IActionResult> CreateCorpus([FromBody] CreateRagCorpusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest("El nombre del corpus es obligatorio.");
            }

            var ragClient = CreateRagClient();
            string projectId = "botforge-499617";
            string location = "us-central1";
            var targetDisplayName = request.DisplayName.Trim();

            try
            {
                var existingCorpusName = await FindCorpusNameByDisplayNameAsync(
                    ragClient,
                    projectId,
                    location,
                    targetDisplayName);

                if (!string.IsNullOrWhiteSpace(existingCorpusName))
                {
                    return Ok(new
                    {
                        message = "El corpus ya existe para esta compañía.",
                        corpusName = existingCorpusName,
                        done = true,
                        alreadyExists = true
                    });
                }

                var rawResponse = await ragClient.CreateRagCorpusAsync(projectId, location, targetDisplayName);
                using var createDoc = JsonDocument.Parse(rawResponse);

                var operationName = createDoc.RootElement.TryGetProperty("name", out var operationNameProperty)
                    ? operationNameProperty.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(operationName))
                {
                    return Ok(new
                    {
                        message = "Solicitud enviada, pero no se pudo leer el operation name.",
                        raw = rawResponse
                    });
                }

                string? finalOperationRaw = rawResponse;
                bool done = false;

                for (var i = 0; i < 10; i++)
                {
                    finalOperationRaw = await ragClient.GetOperationAsync(location, operationName);
                    using var operationDoc = JsonDocument.Parse(finalOperationRaw);

                    done = operationDoc.RootElement.TryGetProperty("done", out var doneProperty) && doneProperty.GetBoolean();
                    if (done)
                    {
                        break;
                    }

                    await Task.Delay(1000);
                }

                if (done && !string.IsNullOrWhiteSpace(finalOperationRaw))
                {
                    using var finalOperationDoc = JsonDocument.Parse(finalOperationRaw);
                    if (finalOperationDoc.RootElement.TryGetProperty("error", out var errorElement)
                        && errorElement.ValueKind == JsonValueKind.Object)
                    {
                        var errorCode = errorElement.TryGetProperty("code", out var codeProperty) ? codeProperty.GetInt32() : 0;
                        var errorMessage = errorElement.TryGetProperty("message", out var messageProperty)
                            ? messageProperty.GetString()
                            : "Error desconocido al crear el corpus.";

                        return StatusCode(502, new
                        {
                            message = "No se pudo crear el corpus en Vertex AI.",
                            operationName,
                            done,
                            error = new
                            {
                                code = errorCode,
                                message = errorMessage
                            },
                            raw = finalOperationRaw
                        });
                    }
                }

                return Ok(new
                {
                    message = done
                        ? "Corpus creado correctamente y ya disponible para listar."
                        : "La creación del corpus sigue en proceso. Intenta listar de nuevo en unos segundos.",
                    operationName,
                    done,
                    raw = finalOperationRaw
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("corpora")]
        public async Task<IActionResult> GetCorpora()
        {
            var ragClient = CreateRagClient();
            string projectId = "botforge-499617";
            string location = "us-central1";

            try
            {
                var rawResponse = await ragClient.ListRagCorporaAsync(projectId, location);
                using var document = JsonDocument.Parse(rawResponse);

                if (document.RootElement.TryGetProperty("ragCorpora", out var corporaElement) && corporaElement.ValueKind == JsonValueKind.Array)
                {
                    var corpora = corporaElement.EnumerateArray().Select(corpus => new
                    {
                        Name = corpus.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null,
                        DisplayName = corpus.TryGetProperty("displayName", out var displayNameProperty) ? displayNameProperty.GetString() : null,
                        CreateTime = corpus.TryGetProperty("createTime", out var createTimeProperty) ? createTimeProperty.GetString() : null
                    }).ToList();

                    return Ok(corpora);
                }

                return Ok(new { raw = rawResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("corpora/{ragCorpusId}/files")]
        public async Task<IActionResult> UploadFilesToCorpus(string ragCorpusId, List<IFormFile> files)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null)
            {
                return BadRequest("No se pudo obtener el companyId desde el token.");
            }

            if (string.IsNullOrWhiteSpace(ragCorpusId))
            {
                return BadRequest("El ragCorpusId es obligatorio.");
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var documentUploadLimit = await _subscriptionPermissionService.GetDocumentUploadLimitAsync(companyId.Value);
            var currentDocumentCount = await _context.DocumentSources.CountAsync(d => d.CompanyId == companyId.Value);
            var incomingFilesCount = files.Count(file => file.Length > 0);

            if (documentUploadLimit.HasValue)
            {
                var availableSlots = Math.Max(documentUploadLimit.Value - currentDocumentCount, 0);
                if (incomingFilesCount > availableSlots)
                {
                    if (documentUploadLimit.Value == 0)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden,
                            $"Tu suscripcion no permite subida de documentacion. Requiere la caracteristica '{SubscriptionFeatureCatalog.LimitedDocumentUpload}' o '{SubscriptionFeatureCatalog.UnlimitedDocumentUpload}'.");
                    }

                    return BadRequest($"Tu plan permite un maximo de {documentUploadLimit.Value} documentos. Actualmente tienes {currentDocumentCount} y solo puedes subir {availableSlots} mas.");
                }
            }

            var ragClient = CreateRagClient();
            string projectId = "botforge-499617";
            string location = "us-central1";
            var bucketName = _configuration["GCS_BUCKET_NAME"]
                ?? _configuration["GCP:RagBucket"]
                ?? _configuration["GcsBucketName"];

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return BadRequest("Configura GCS_BUCKET_NAME (o GCP:RagBucket/GcsBucketName) para subir archivos al bucket.");
            }

            var uploadResults = new List<object>();
            var companyFolderPrefix = $"company-{companyId.Value}";
            var companyFolderUri = $"gs://{bucketName}/{companyFolderPrefix}/";

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    continue;
                }

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
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedDocuments", companyId.Value.ToString());
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

                    var gcsObjectName = $"{companyFolderPrefix}/{uniqueFileName}";
                    var gcsUri = await ragClient.UploadFileToGcsAsync(bucketName, gcsObjectName, filePath, file.ContentType);

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
                        Uri = gcsUri,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.DocumentSources.Add(documentSource);
                    await _context.SaveChangesAsync();

                    uploadResults.Add(new
                    {
                        file = file.FileName,
                        gcsUri
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error subiendo {file.FileName}: {ex.Message}");
                }
            }
            try
            {
                StartRebuildCompanyCorpusInBackground(
                    projectId,
                    location,
                    companyId.Value,
                    bucketName);

                return Accepted(new
                {
                    message = "Archivos subidos. La reconstruccion del corpus se ejecuta en batch.",
                    corpusId = ragCorpusId,
                    companyFolder = companyFolderUri,
                    files = uploadResults,
                    rebuildStarted = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error iniciando la reconstruccion del corpus: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("files")]
        public async Task<IActionResult> DeleteFiles([FromBody] DeleteDocumentSourcesRequest request)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null)
            {
                return BadRequest("No se pudo obtener el companyId desde el token.");
            }

            if (request == null)
            {
                return BadRequest("Debes enviar un body valido.");
            }

            var normalizedIds = (request.DocumentSourceIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalizedUris = (request.DocumentUris ?? new List<string>())
                .Where(uri => !string.IsNullOrWhiteSpace(uri))
                .Select(uri => uri.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (normalizedIds.Count == 0 && normalizedUris.Count == 0)
            {
                return BadRequest("Debes enviar al menos un documentSourceId o documentUri valido.");
            }

            var documents = await _context.DocumentSources
                .Where(d => d.CompanyId == companyId.Value
                    && (normalizedIds.Contains(d.Id) || normalizedUris.Contains(d.Uri)))
                .ToListAsync();

            var foundIds = documents.Select(d => d.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var notFoundIds = normalizedIds.Where(id => !foundIds.Contains(id)).ToList();
            var foundUris = documents.Select(d => d.Uri).ToHashSet(StringComparer.Ordinal);
            var notFoundUris = normalizedUris.Where(uri => !foundUris.Contains(uri)).ToList();

            if (documents.Count == 0)
            {
                return NotFound(new
                {
                    message = "No se encontraron documentos de esta compania para los IDs/URIs enviados.",
                    notFoundIds,
                    notFoundUris
                });
            }

            var ragClient = CreateRagClient();
            string projectId = "botforge-499617";
            string location = "us-central1";
            var bucketName = _configuration["GCS_BUCKET_NAME"]
                ?? _configuration["GCP:RagBucket"]
                ?? _configuration["GcsBucketName"];

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return BadRequest("Configura GCS_BUCKET_NAME (o GCP:RagBucket/GcsBucketName) para eliminar y reconstruir corpus.");
            }

            Dictionary<string, bool> corpusDeleteByUri = documents
                .Select(d => d.Uri)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToDictionary(uri => uri, _ => false, StringComparer.OrdinalIgnoreCase);

            try
            {
                var corpusName = await FindCorpusNameByDisplayNameAsync(
                    ragClient,
                    projectId,
                    location,
                    $"company-{companyId.Value}");

                if (!string.IsNullOrWhiteSpace(corpusName))
                {
                    corpusDeleteByUri = await ragClient.DeleteRagFilesByGcsUrisAsync(
                        projectId,
                        location,
                        corpusName,
                        documents.Select(d => d.Uri));
                }
            }
            catch
            {
            
            }

            var failed = new List<object>();
            var deleted = new List<object>();

            foreach (var doc in documents)
            {
                var corpusDeleted = corpusDeleteByUri.TryGetValue(doc.Uri, out var removedFromCorpus) && removedFromCorpus;
                var bucketDeleted = false;
                string? bucketError = null;

                try
                {
                    bucketDeleted = await ragClient.DeleteGcsObjectByUriAsync(doc.Uri);
                    if (!bucketDeleted)
                    {
                        bucketError = "No se pudo verificar la eliminación del archivo en GCS después de varios intentos.";
                    }
                }
                catch (Exception ex)
                {
                    bucketError = ex.Message;
                }

                if (bucketDeleted)
                {
                    _context.DocumentSources.Remove(doc);
                    deleted.Add(new
                    {
                        id = doc.Id,
                        uri = doc.Uri,
                        corpusDeleted
                    });
                    continue;
                }

                failed.Add(new
                {
                    id = doc.Id,
                    uri = doc.Uri,
                    corpusDeleted,
                    bucketDeleted,
                    bucketError,
                    message = corpusDeleted
                        ? "No se pudo eliminar el archivo del bucket."
                        : "No se pudo eliminar el archivo del corpus."
                });
            }

            if (deleted.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            StartRebuildCompanyCorpusInBackground(
                projectId,
                location,
                companyId.Value,
                bucketName);

            return Ok(new
            {
                message = failed.Count == 0
                    ? "Archivos eliminados. La reconstruccion del corpus se ejecuta en batch."
                    : "Se eliminaron algunos archivos y otros fallaron. La reconstruccion del corpus se ejecuta en batch con el estado actual del bucket.",
                deleted,
                failed,
                notFoundIds,
                notFoundUris,
                rebuildStarted = true
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("rebuild-status")]
        public IActionResult GetRebuildStatus()
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null)
            {
                return BadRequest("No se pudo obtener el companyId desde el token.");
            }

            if (_rebuildStatusByCompany.TryGetValue(companyId.Value, out var status))
            {
                return Ok(status);
            }

            return Ok(new CorpusRebuildStatusResponse
            {
                Status = "idle",
                Message = "No hay reconstrucciones en curso."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UploadDocumentSource(List<IFormFile> files)
        {
            var companyId = GetCompanyIdFromToken();
            if (companyId == null)
            {
                return BadRequest("No se pudo obtener el companyId desde el token.");
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var ragClient = CreateRagClient();
            string projectId = "botforge-499617";
            string location = "us-central1";

            try
            {
                var ragCorpusId = await GetOrCreateCompanyCorpusAsync(ragClient, projectId, location, companyId.Value);
                return await UploadFilesToCorpus(ragCorpusId, files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error preparando corpus de empresa: {ex.Message}");
            }
        }

        private async Task<string> GetOrCreateCompanyCorpusAsync(VertexAiRagClient ragClient, string projectId, string location, int companyId)
        {
            var targetDisplayName = $"company-{companyId}";

            var existingCorpusName = await FindCorpusNameByDisplayNameAsync(ragClient, projectId, location, targetDisplayName);
            if (!string.IsNullOrWhiteSpace(existingCorpusName))
            {
                return existingCorpusName;
            }

            var createRaw = await ragClient.CreateRagCorpusAsync(projectId, location, targetDisplayName);
            using var createDoc = JsonDocument.Parse(createRaw);
            var operationName = createDoc.RootElement.TryGetProperty("name", out var operationNameProperty)
                ? operationNameProperty.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(operationName))
            {
                for (var i = 0; i < 10; i++)
                {
                    var finalOperationRaw = await ragClient.GetOperationAsync(location, operationName);
                    using var operationDoc = JsonDocument.Parse(finalOperationRaw);

                    var done = operationDoc.RootElement.TryGetProperty("done", out var doneProperty) && doneProperty.GetBoolean();
                    if (done)
                    {
                        if (operationDoc.RootElement.TryGetProperty("error", out var errorElement)
                            && errorElement.ValueKind == JsonValueKind.Object)
                        {
                            var errorMessage = errorElement.TryGetProperty("message", out var messageProperty)
                                ? messageProperty.GetString()
                                : "Error desconocido creando corpus.";
                            throw new InvalidOperationException(errorMessage);
                        }
                        break;
                    }

                    await Task.Delay(1000);
                }
            }

            var createdCorpusName = await FindCorpusNameByDisplayNameAsync(ragClient, projectId, location, targetDisplayName);
            if (string.IsNullOrWhiteSpace(createdCorpusName))
            {
                throw new InvalidOperationException("No se pudo resolver el corpus de la empresa después de crearlo.");
            }

            return createdCorpusName;
        }

        private async Task<string> RebuildCompanyCorpusAsync(
            VertexAiRagClient ragClient,
            string projectId,
            string location,
            int companyId,
            string bucketName,
            AppDbContext dbContext)
        {
            var displayName = $"company-{companyId}";
            var folderPrefix = $"company-{companyId}/";

            var existingCorpusName = await FindCorpusNameByDisplayNameAsync(ragClient, projectId, location, displayName);
            if (!string.IsNullOrWhiteSpace(existingCorpusName))
            {
                try
                {
                    var deleteRaw = await ragClient.DeleteRagCorpusAsync(location, existingCorpusName);
                    await WaitOperationCompletionAsync(ragClient, location, deleteRaw, "Error eliminando corpus.");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains(((int)HttpStatusCode.NotFound).ToString(), StringComparison.OrdinalIgnoreCase))
                {
                
                }
            }

            var newCorpusName = await GetOrCreateCompanyCorpusAsync(ragClient, projectId, location, companyId);

            var urisToImport = await SyncBucketWithDatabaseAsync(
                ragClient,
                companyId,
                bucketName,
                folderPrefix,
                dbContext);

            if (urisToImport.Count == 0)
            {
                return newCorpusName;
            }

            var importRaw = await ragClient.ImportRagFilesAsync(projectId, location, newCorpusName, urisToImport);
            await WaitOperationCompletionAsync(ragClient, location, importRaw, "Error importando archivos al corpus reconstruido.");

            return newCorpusName;
        }

        private async Task<List<string>> SyncBucketWithDatabaseAsync(
            VertexAiRagClient ragClient,
            int companyId,
            string bucketName,
            string folderPrefix,
            AppDbContext dbContext)
        {
            var dbUris = await dbContext.DocumentSources
                .AsNoTracking()
                .Where(d => d.CompanyId == companyId)
                .Select(d => d.Uri)
                .Where(uri => !string.IsNullOrWhiteSpace(uri))
                .Distinct()
                .ToListAsync();

            var dbUriSet = new HashSet<string>(dbUris, StringComparer.Ordinal);
            var bucketUris = await ragClient.ListGcsUrisByPrefixAsync(bucketName, folderPrefix);

        
            var staleBucketUris = bucketUris
                .Where(uri => !dbUriSet.Contains(uri))
                .ToList();

            foreach (var staleUri in staleBucketUris)
            {
                try
                {
                    await ragClient.DeleteGcsObjectByUriAsync(staleUri);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BucketSync][company-{companyId}] No se pudo eliminar objeto obsoleto {staleUri}: {ex.Message}");
                }
            }

            var finalBucketUris = await ragClient.ListGcsUrisByPrefixAsync(bucketName, folderPrefix);
            var finalBucketUriSet = new HashSet<string>(finalBucketUris, StringComparer.Ordinal);

            return dbUris
                .Where(uri => finalBucketUriSet.Contains(uri))
                .ToList();
        }

        private void StartRebuildCompanyCorpusInBackground(
            string projectId,
            string location,
            int companyId,
            string bucketName)
        {
            _rebuildStatusByCompany[companyId] = new CorpusRebuildStatusResponse
            {
                Status = "running",
                StartedAtUtc = DateTime.UtcNow,
                Message = "Reconstruyendo corpus en segundo plano..."
            };

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var ragClient = CreateRagClient();
                    var corpusName = await RebuildCompanyCorpusAsync(
                        ragClient,
                        projectId,
                        location,
                        companyId,
                        bucketName,
                        scopedDbContext);

                    _rebuildStatusByCompany[companyId] = new CorpusRebuildStatusResponse
                    {
                        Status = "succeeded",
                        StartedAtUtc = _rebuildStatusByCompany.TryGetValue(companyId, out var current) ? current.StartedAtUtc : DateTime.UtcNow,
                        FinishedAtUtc = DateTime.UtcNow,
                        Message = "Corpus reconstruido correctamente.",
                        CorpusName = corpusName
                    };
                }
                catch (Exception ex)
                {
                    _rebuildStatusByCompany[companyId] = new CorpusRebuildStatusResponse
                    {
                        Status = "failed",
                        StartedAtUtc = _rebuildStatusByCompany.TryGetValue(companyId, out var current) ? current.StartedAtUtc : DateTime.UtcNow,
                        FinishedAtUtc = DateTime.UtcNow,
                        Message = ex.Message
                    };
                    Console.WriteLine($"[CorpusRebuild][company-{companyId}] Error en batch: {ex.Message}");
                }
            });
        }

        private static async Task WaitOperationCompletionAsync(
            VertexAiRagClient ragClient,
            string location,
            string initialOperationRaw,
            string defaultErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(initialOperationRaw))
            {
                return;
            }

            using var initialDoc = JsonDocument.Parse(initialOperationRaw);
            var operationName = initialDoc.RootElement.TryGetProperty("name", out var opNameProperty)
                ? opNameProperty.GetString()
                : null;

            if (string.IsNullOrWhiteSpace(operationName))
            {
                return;
            }

            string? finalRaw = initialOperationRaw;
            for (var i = 0; i < 300; i++)
            {
                finalRaw = await ragClient.GetOperationAsync(location, operationName);
                using var operationDoc = JsonDocument.Parse(finalRaw);

                var done = operationDoc.RootElement.TryGetProperty("done", out var doneProperty) && doneProperty.GetBoolean();
                if (!done)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (operationDoc.RootElement.TryGetProperty("error", out var errorElement)
                    && errorElement.ValueKind == JsonValueKind.Object)
                {
                    var errorMessage = errorElement.TryGetProperty("message", out var messageProperty)
                        ? messageProperty.GetString()
                        : defaultErrorMessage;

                    throw new InvalidOperationException(errorMessage ?? defaultErrorMessage);
                }

                return;
            }

            throw new TimeoutException("La operación de Vertex AI no terminó a tiempo.");
        }

        private static async Task<string?> FindCorpusNameByDisplayNameAsync(VertexAiRagClient ragClient, string projectId, string location, string displayName)
        {
            var listRaw = await ragClient.ListRagCorporaAsync(projectId, location);
            using var listDoc = JsonDocument.Parse(listRaw);

            if (!listDoc.RootElement.TryGetProperty("ragCorpora", out var corporaElement)
                || corporaElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            string? selectedName = null;
            DateTimeOffset? selectedCreateTime = null;

            foreach (var corpus in corporaElement.EnumerateArray())
            {
                var currentDisplayName = corpus.TryGetProperty("displayName", out var displayNameProperty)
                    ? displayNameProperty.GetString()
                    : null;

                if (!string.Equals(currentDisplayName, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var currentName = corpus.TryGetProperty("name", out var nameProperty)
                    ? nameProperty.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(currentName))
                {
                    continue;
                }

                DateTimeOffset? currentCreateTime = null;
                if (corpus.TryGetProperty("createTime", out var createTimeProperty))
                {
                    var createTimeText = createTimeProperty.GetString();
                    if (DateTimeOffset.TryParse(createTimeText, out var parsedCreateTime))
                    {
                        currentCreateTime = parsedCreateTime;
                    }
                }

                if (selectedName == null)
                {
                    selectedName = currentName;
                    selectedCreateTime = currentCreateTime;
                    continue;
                }

                if (currentCreateTime.HasValue && (!selectedCreateTime.HasValue || currentCreateTime.Value > selectedCreateTime.Value))
                {
                    selectedName = currentName;
                    selectedCreateTime = currentCreateTime;
                }
            }

            return selectedName;
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