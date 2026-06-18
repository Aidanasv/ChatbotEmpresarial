namespace backend.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public int ConversationId { get; set; }
    }

    public class ChatResponseDto
    {
        public string Response { get; set; } = string.Empty;
    }

    public class ConversationDto
    {
        public int ChatbotId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
    }

    public class ChatbotEmbedConfigDto
    {
        public int ChatbotId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public bool ShowChatbotAvatar { get; set; }
        public bool WidgetPosition { get; set; }
    }

    public class ConversationMensageDto
    {
        public int Id { get; set; }
        public int ChatbotSettingsId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<MenssageDto> Messages { get; set; } = new List<MenssageDto>();
    }

    public class MenssageDto
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}