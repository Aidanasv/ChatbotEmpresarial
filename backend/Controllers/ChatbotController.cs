using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatbotController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(ChatRequestDto request)
        {
            var conversation = await _context.Conversations
                .Include(c => c.ChatbotSettings)
                .ThenInclude(cs => cs.Company)
                .ThenInclude(c => c.Faqs)
                .Include(c => c.ChatbotSettings)
                .ThenInclude(cs => cs.Company)
                .ThenInclude(c => c.DocumentSources)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
            {
                return NotFound();
            }
            var chatbotSettings = conversation.ChatbotSettings;
            var company = conversation.ChatbotSettings?.Company;
            var client = new Client();

            var prompt = $@"
                Eres {chatbotSettings?.ChatbotName}, un asistente virtual de atención al cliente profesional para la empresa '{company?.Name}'.
                Tu objetivo es ayudar a los usuarios resolviendo sus dudas con la información provista.

                [PERSONALIDAD Y TONO]
                - Debes responder estrictamente con un tono {chatbotSettings?.ChatbotTone.ToString().ToLower()}.
                - Saludo inicial requerido: {chatbotSettings?.GreetingMessage}

                [REGLAS DE RESPUESTA CRÍTICAS]
                1. Usa ÚNICAMENTE la 'Base de conocimiento' (FAQs y Documentos adjuntos) proporcionada para responder.
                2. Si la respuesta no se encuentra explícitamente en la Base de conocimiento, debes responder textualmente con tu mensaje de escape: '{chatbotSettings?.FallbackMessage}'. NO inventes información bajo ningún concepto.
                3. Sé conciso, directo y mantén la consistencia de la marca.

                [BASE DE CONOCIMIENTO - PREGUNTAS FRECUENTES (FAQs)]
                ";
            Console.WriteLine(prompt);
            foreach (var faq in company?.Faqs ?? Enumerable.Empty<Faq>())
            {
                prompt += $"\n- Pregunta: {faq.Question} -> Respuesta: {faq.Answer}";
            }

            prompt += "\n\n[BASE DE CONOCIMIENTO - DOCUMENTOS ADJUNTOS]\n";


            var history = await _context.Messages
                .Where(m => m.ConversationId == request.ConversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            var messages = new List<Content>();
            foreach (var message in history)
            {
                messages.Add(new Content
                {
                    Parts = new List<Part>
                    {
                        new Part { Text = message.Content }
                    },
                    Role = message.Role == RoleInConversation.User ? "user" : "model"
                });
            }

            foreach (var file in company?.DocumentSources ?? Enumerable.Empty<DocumentSource>())
            {
                messages.Add(new Content
                {
                    Parts = new List<Part>
                    {
                        new Part { FileData = new FileData
                        {
                            FileUri = file.Uri,
                            MimeType = "application/pdf"
                    
                        } }
                    },
                    Role = "user"
                });
            }

            messages.Add(new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = request.Message }
                },
                Role = "user"
            });

            await _context.Messages.AddAsync(new Message
            {
                ConversationId = request.ConversationId,
                Role = RoleInConversation.User,
                Content = request.Message
            });

            var responseGoogle = await client.Models.GenerateContentAsync(
              model: "gemini-flash-latest",
              contents: messages,
              config: new GenerateContentConfig
              {
                  SystemInstruction = new Content
                  {
                      Parts = new List<Part> {
                        new Part {Text = prompt}
                        }
                  },
                  Temperature = 0.4,
                  MaxOutputTokens = 1000
              });


            var response = new ChatResponseDto
            {
                Response = responseGoogle.Candidates[0].Content.Parts[0].Text
            };

            await _context.Messages.AddAsync(new Message
            {
                ConversationId = request.ConversationId,
                Role = RoleInConversation.Bot,
                Content = response.Response
            });
            await _context.SaveChangesAsync();
            return Ok(response);

        }

        [HttpPost("start-conversation")]
        public async Task<IActionResult> StartConversation(ConversationDto conversationDto)
        {
            var conversation = new Conversation
            {
                ChatbotSettingsId = conversationDto.ChatbotId,
                CustomerEmail = conversationDto.CustomerEmail,
                CustomerName = conversationDto.CustomerName,
                CustomerPhone = conversationDto.CustomerPhone,
                Topic = conversationDto.Topic,
                Status = ConversationStatus.Open
            };
            var chatbotSettings = await _context.ChatbotSettings
                .Include(cs => cs.Company)
                .ThenInclude(c => c.DocumentSources)
                .FirstOrDefaultAsync(cs => cs.Id == conversationDto.ChatbotId);


            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var client = new Client();
            foreach (var doc in chatbotSettings?.Company?.DocumentSources ?? Enumerable.Empty<DocumentSource>())
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedDocuments", chatbotSettings.Company.Id.ToString(), doc.Id);

                var fileUploadResponse = await client.Files.UploadAsync(filePath);
                doc.Uri = fileUploadResponse.Uri;
                _context.DocumentSources.Update(doc);

            }

            await _context.SaveChangesAsync();
            return Ok(new { ConversationId = conversation.Id });
        }
    }
}