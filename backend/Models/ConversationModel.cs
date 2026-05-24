using System.Data;

namespace backend.Models

{
    public class Conversation
    {
        public int Id { get; set; }
        public int ChatbotSettingsId { get; set; }
        public ChatbotSettings? ChatbotSettings { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public ConversationStatus Status { get; set; } = ConversationStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
        public RoleInConversation Role { get; set; } = RoleInConversation.User;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}