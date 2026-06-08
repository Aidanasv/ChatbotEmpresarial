namespace backend.DTOs
{
    public class AnalyticsSuperAdminDTO
    {
        public int TotalCompanies { get; set; }
        public int TotalUsers { get; set; }
        public int TotalConversations { get; set; }
        public double MRR { get; set; }
    }
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

    public class CompanyAnalyticsDTO
    {
        public int CompanyId { get; set; }
        public string Initials { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanySubscription { get; set; } = string.Empty;
        public int Users { get; set; }
        public int Conversations { get; set; }
        public double MRR { get; set; } = 0.0;
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}