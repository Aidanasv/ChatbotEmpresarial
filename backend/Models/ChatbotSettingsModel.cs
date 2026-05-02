namespace backend.Models
{
    public class ChatbotSettings
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public string ChatbotName { get; set; } = "Chatbot";
        public string PrimaryColor { get; set; } = "#007bff";
        public ChatbotTone ChatbotTone { get; set; } = ChatbotTone.Friendly;
        public string GreetingMessage { get; set; } = "Hola, ¿en qué puedo ayudarte hoy?";
        public string FallbackMessage { get; set; } = "Lo siento, no entendí eso. ¿Podrías reformular tu pregunta?";
        public bool ShowAvatar { get; set; } = true;
        public bool WidgetPosition { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
      
    }               
}