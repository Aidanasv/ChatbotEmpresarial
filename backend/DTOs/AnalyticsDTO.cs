namespace backend.DTOs
{
    public class AnalyticsDTO
    {
        public int TotalConversations { get; set; }
        public int TotalActiveUsers { get; set; }
        public int AverageMessagesByConversation { get; set; }
        public double AverageResponseTime { get; set; }
    }

    public class ConversationAnalyticsDTO
    {
        public int ConversationId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string LastMessageTime { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

    }
}