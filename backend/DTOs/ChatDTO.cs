namespace backend.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public int  ConversationId { get; set; }
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
}