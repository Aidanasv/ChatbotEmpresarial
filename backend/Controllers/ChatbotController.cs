using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Google.GenAI;
using Google.GenAI.Types;
using backend.Models;
using Microsoft.EntityFrameworkCore;

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
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

            if (conversation == null)
            {
                return NotFound();
            }
            var chatbotSettings = conversation.ChatbotSettings;
            var company = conversation.ChatbotSettings?.Company;

            var prompt = $"Eres {chatbotSettings?.ChatbotName}, un asistente de atención al cliente para una empresa {company?.Name}. Responde de manera {chatbotSettings?.ChatbotTone.ToString().ToLower()}. Tu saludo inicial debe ser {chatbotSettings?.GreetingMessage} y si no sabes que responder debes decir {chatbotSettings?.FallbackMessage}. Aquí hay algunas preguntas frecuentes de la empresa:";
            Console.WriteLine(prompt);
            foreach (var faq in company?.Faqs ?? Enumerable.Empty<Faq>())
            {
                prompt += $"Pregunta: {faq.Question} Respuesta: {faq.Answer}. ";
            }

            var client = new Client();
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

            messages.Add(new Content
            {
                Parts = new List<Part>
                {
                    new Part { Text = request.Message }
                },
                Role = "user"
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
                Role = RoleInConversation.User,
                Content = request.Message
            });

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

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return Ok(new { ConversationId = conversation.Id });
        }
    }
}