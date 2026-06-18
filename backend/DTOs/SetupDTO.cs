using backend.Models;

namespace backend.DTOs
{
    public class CompanySetupDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Cif { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CompanySetupResponseDto : CompanySetupDto
    {
        public int Id { get; set; }
        public DateTime RenewalDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PersonalitySetupDto
    {
        public string ChatbotName { get; set; } = string.Empty;
        public ChatbotTone ChatbotTone { get; set; } = ChatbotTone.Friendly;
        public string GreetingMessage { get; set; } = "Hola, ¿en qué puedo ayudarte hoy?";
        public string FallbackMessage { get; set; } = "Lo siento, no entendí eso. ¿Podrías reformular tu pregunta?";
    }

    public class AppearanceSetupDto
    {
        public string PrimaryColor { get; set; } = "#007bff";
        public bool ShowChatbotAvatar { get; set; } = true;
        public bool WidgetPosition { get; set; } = true;
    }

    public class SetupResponseDto
    {
        public CompanySetupDto CompanySetup { get; set; } = new CompanySetupDto();
        public PersonalitySetupDto PersonalitySetup { get; set; } = new PersonalitySetupDto();
        public AppearanceSetupDto AppearanceSetup { get; set; } = new AppearanceSetupDto();
        public KnowledgeSetupDto KnowledgeSetup { get; set; } = new KnowledgeSetupDto();
    }

    public class UpdateCompanyStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    
    public class KnowledgeSetupDto
    {
        public List<FaqResponseDto> Faqs { get; set; } = new List<FaqResponseDto>();
        public List<DocumentSourceResponseDto> Documents { get; set; } = new List<DocumentSourceResponseDto>();

    }

}