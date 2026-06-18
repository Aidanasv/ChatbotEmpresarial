using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using GoogleCloud.VertexAI;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public ChatbotController(
            AppDbContext context,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        private static VertexAiRagClient CreateRagClient()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<VertexAiRagClient>();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<VertexAiRagClient>();
        }

        [HttpGet("embed/{chatbotId}")]
        public async Task<IActionResult> GetEmbedConfig(int chatbotId)
        {
            var chatbotSettings = await _context.ChatbotSettings
                .Include(cs => cs.Company)
                .FirstOrDefaultAsync(cs => cs.Id == chatbotId);

            if (chatbotSettings == null || chatbotSettings.Company == null)
            {
                return NotFound("Chatbot no encontrado.");
            }

            if (!IsOriginAllowedForChatbot(chatbotSettings.Company.Website))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Origen no permitido para este chatbot.");
            }

            if (!IsCompanyActive(chatbotSettings.Company))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Este chatbot solo puede embeberse para empresas activas.");
            }

            var config = new ChatbotEmbedConfigDto
            {
                ChatbotId = chatbotSettings.Id,
                Title = chatbotSettings.ChatbotName,
                PrimaryColor = chatbotSettings.PrimaryColor,
                ShowChatbotAvatar = chatbotSettings.ShowAvatar,
                WidgetPosition = chatbotSettings.WidgetPosition
            };

            return Ok(config);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(ChatRequestDto request)
        {
            var conversation = await _context.Conversations
                .Include(c => c.ChatbotSettings)
                .ThenInclude(cs => cs!.Company)
                .ThenInclude(company => company!.Faqs)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
            {
                return NotFound();
            }

            if (!IsOriginAllowedForChatbot(conversation.ChatbotSettings?.Company?.Website))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Origen no permitido para este chatbot.");
            }

            if (IsEmbedRequest() && !IsCompanyActive(conversation.ChatbotSettings?.Company))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Este chatbot solo puede embeberse para empresas activas.");
            }

            var chatbotSettings = conversation.ChatbotSettings;
            var company = conversation.ChatbotSettings?.Company;
            var ragClient = CreateRagClient();

            string projectId = _configuration["GCP:ProjectId"] ?? "botforge-499617";
            string location = _configuration["GCP:Location"] ?? "us-central1";
            string modelId = _configuration["GCP:ModelId"] ?? "gemini-3.5-flash";
            const double vectorDistanceThreshold = 0.6;
            const int similarityTopK = 5;

            var ragContextText = string.Empty;
            if (company != null)
            {
                var corpusDisplayName = $"company-{company.Id}";
                var corpusResourceName = await FindCorpusNameByDisplayNameAsync(ragClient, projectId, location, corpusDisplayName);
                if (!string.IsNullOrWhiteSpace(corpusResourceName))
                {
                    var retrieveRaw = await ragClient.RetrieveContextsAsync(
                        projectId,
                        location,
                        corpusResourceName,
                        request.Message,
                        vectorDistanceThreshold,
                        similarityTopK);

                    ragContextText = BuildRagContextText(retrieveRaw);
                }
            }

            var prompt = $@"
                Eres {chatbotSettings?.ChatbotName}, un asistente virtual de atención al cliente profesional
                para la empresa '{company?.Name}': el cif es {company?.CIF} y el correo de contacto es {company?.Email}
                la pagina web es {company?.Website} y el nombre legal es {company?.LegalName}.
                la descripción de la empresa es {company?.Description}.

                Tu objetivo es ayudar a los usuarios resolviendo sus dudas con la información provista.

                [PERSONALIDAD Y TONO]
                - Debes responder estrictamente con un tono {chatbotSettings?.ChatbotTone.ToString().ToLower()}.
                - En el saludo inicial, transmite la intención de ayuda de forma natural y variada, sin repetir siempre la misma frase literal.
                - Puedes usar como referencia este estilo de saludo: '{chatbotSettings?.GreetingMessage}'.

                [REGLAS DE RESPUESTA CRÍTICAS]
                1. Usa ÚNICAMENTE la información validada disponible (FAQs y contexto documental recuperado) para responder.
                2. Si la respuesta no se encuentra explícitamente en la información disponible, responde con una variante natural del mensaje de escape '{chatbotSettings?.FallbackMessage}', manteniendo el mismo sentido. NO inventes información bajo ningún concepto.
                3. Sé conciso, directo y mantén la consistencia de la marca.
                4. Nunca expliques al usuario detalles internos del sistema (por ejemplo: 'base de conocimiento', 'RAG', 'documentos recuperados', 'contexto interno'). Habla de forma transparente y orientada a ayudar.

                [BASE DE CONOCIMIENTO - PREGUNTAS FRECUENTES (FAQs)]
                ";
            Console.WriteLine(prompt);
            foreach (var faq in company?.Faqs ?? Enumerable.Empty<Faq>())
            {
                prompt += $"\n- Pregunta: {faq.Question} -> Respuesta: {faq.Answer}";
            }

            prompt += "\n\n[BASE DE CONOCIMIENTO - CONTEXTO RECUPERADO DE DOCUMENTOS (RAG)]\n";
            prompt += string.IsNullOrWhiteSpace(ragContextText)
                ? "No se recuperó contexto documental para esta consulta."
                : ragContextText;


            var history = await _context.Messages
                .Where(m => m.ConversationId == request.ConversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            var contents = new List<object>();
            foreach (var message in history)
            {
                contents.Add(new
                {
                    role = message.Role == RoleInConversation.User ? "user" : "model",
                    parts = new[]
                    {
                        new { text = message.Content }
                    }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = new[]
                {
                    new { text = request.Message }
                }
            });
            prompt += $"IMPORTANTE: si el usuario te pregunta sobre la empresa, productos o servicios, busca en esta url: {company?.Website}";

            var generationRaw = await ragClient.GenerateContentAsync(
                projectId,
                location,
                modelId,
                prompt,
                contents: contents,
                temperature: 0.4,
                maxOutputTokens: 5000);

            var response = new ChatResponseDto
            {
                Response = ExtractGeneratedText(generationRaw)
            };

            SaveMessagesInBackground(request.ConversationId, request.Message, response.Response);
            return Ok(response);

        }

        private void SaveMessagesInBackground(int conversationId, string userMessage, string botMessage)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    await scopedContext.Messages.AddRangeAsync(
                        new Message
                        {
                            ConversationId = conversationId,
                            Role = RoleInConversation.User,
                            Content = userMessage
                        },
                        new Message
                        {
                            ConversationId = conversationId,
                            Role = RoleInConversation.Bot,
                            Content = botMessage
                        });

                    await scopedContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ChatbotController] Error guardando mensajes en segundo plano para conversación {conversationId}: {ex.Message}");
                }
            });
        }

        [HttpPost("start-conversation")]
        public async Task<IActionResult> StartConversation(ConversationDto conversationDto)
        {
            var chatbotSettings = await _context.ChatbotSettings
                .Include(cs => cs.Company)
                .ThenInclude(company => company!.DocumentSources)
                .FirstOrDefaultAsync(cs => cs.Id == conversationDto.ChatbotId);

            if (chatbotSettings == null || chatbotSettings.Company == null)
            {
                return NotFound("Chatbot no encontrado.");
            }

            if (!IsOriginAllowedForChatbot(chatbotSettings.Company.Website))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Origen no permitido para este chatbot.");
            }

            if (IsEmbedRequest() && !IsCompanyActive(chatbotSettings.Company))
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Este chatbot solo puede embeberse para empresas activas.");
            }

            var conversation = new Conversation
            {
                ChatbotSettingsId = conversationDto.ChatbotId,
                CustomerEmail = conversationDto.CustomerEmail,
                CustomerName = conversationDto.CustomerName,
                CustomerPhone = conversationDto.CustomerPhone,
                Topic = conversationDto.Topic,
                Status = ConversationStatus.Open
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return Ok(new { ConversationId = conversation.Id });
        }

        private bool IsRequestOriginAllowed(string? allowedWebsite)
        {
            var requestOrigin = GetRequestOrigin();
            if (string.IsNullOrWhiteSpace(requestOrigin) || string.IsNullOrWhiteSpace(allowedWebsite))
            {
                return false;
            }

            if (!TryGetOriginUri(requestOrigin, out var requestUri) || !TryGetOriginUri(allowedWebsite, out var allowedUri))
            {
                return false;
            }

            if (!string.Equals(requestUri.Host, allowedUri.Host, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!allowedUri.IsDefaultPort && requestUri.Port != allowedUri.Port)
            {
                return false;
            }

            return true;
        }

        private bool IsEmbedRequest()
        {
            var embedOrigin = Request.Headers["X-Embed-Origin"].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(embedOrigin);
        }

        private static bool IsCompanyActive(Company? company)
        {
            return company?.Status == CompanyStatus.Active;
        }

        private bool IsOriginAllowedForChatbot(string? companyWebsite)
        {
            if (IsRequestOriginAllowed(companyWebsite))
            {
                return true;
            }

            var frontendAllowedOrigins = _configuration.GetSection("FrontendAllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            foreach (var frontendOrigin in frontendAllowedOrigins)
            {
                if (IsRequestOriginAllowed(frontendOrigin))
                {
                    return true;
                }
            }

            return false;
        }

        private string? GetRequestOrigin()
        {
            var embedOrigin = Request.Headers["X-Embed-Origin"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(embedOrigin))
            {
                return embedOrigin;
            }

            var origin = Request.Headers["Origin"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(origin))
            {
                return origin;
            }

            return null;
        }

        private static bool TryGetOriginUri(string raw, out Uri originUri)
        {
            originUri = null!;
            var value = raw.Trim();

            if (!value.StartsWith("http://", true, CultureInfo.InvariantCulture)
                && !value.StartsWith("https://", true, CultureInfo.InvariantCulture))
            {
                value = $"https://{value}";
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var parsed))
            {
                return false;
            }

            originUri = new Uri(parsed.GetLeftPart(UriPartial.Authority));
            return true;
        }

        private static string ExtractGeneratedText(string generationRaw)
        {
            using var document = JsonDocument.Parse(generationRaw);

            if (!document.RootElement.TryGetProperty("candidates", out var candidatesElement)
                || candidatesElement.ValueKind != JsonValueKind.Array
                || candidatesElement.GetArrayLength() == 0)
            {
                return "No se pudo generar una respuesta en este momento.";
            }

            var firstCandidate = candidatesElement[0];
            if (!firstCandidate.TryGetProperty("content", out var contentElement)
                || contentElement.ValueKind != JsonValueKind.Object
                || !contentElement.TryGetProperty("parts", out var partsElement)
                || partsElement.ValueKind != JsonValueKind.Array)
            {
                return "No se pudo generar una respuesta en este momento.";
            }

            foreach (var part in partsElement.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var textElement) && !string.IsNullOrWhiteSpace(textElement.GetString()))
                {
                    return textElement.GetString()!;
                }
            }

            return "No se pudo generar una respuesta en este momento.";
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

            foreach (var corpus in corporaElement.EnumerateArray())
            {
                var currentDisplayName = corpus.TryGetProperty("displayName", out var displayNameProperty)
                    ? displayNameProperty.GetString()
                    : null;

                if (!string.Equals(currentDisplayName, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return corpus.TryGetProperty("name", out var nameProperty)
                    ? nameProperty.GetString()
                    : null;
            }

            return null;
        }

        private static string BuildRagContextText(string retrieveRaw)
        {
            using var document = JsonDocument.Parse(retrieveRaw);
            var lines = new List<string>();

            if (!document.RootElement.TryGetProperty("contexts", out var contextsElement)
                || contextsElement.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            if (!contextsElement.TryGetProperty("contexts", out var itemsElement)
                || itemsElement.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            foreach (var item in itemsElement.EnumerateArray())
            {
                var sourceUri = item.TryGetProperty("sourceUri", out var sourceUriProperty)
                    ? sourceUriProperty.GetString()
                    : "sin_origen";
                var text = item.TryGetProperty("text", out var textProperty)
                    ? textProperty.GetString()
                    : null;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    lines.Add($"- [{sourceUri}] {text}");
                }
            }

            return string.Join("\n", lines);
        }
    }
}