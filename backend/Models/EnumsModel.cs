namespace backend.Models
{
    public enum Role { SuperAdmin, Admin, User }
    public enum UserStatus { Active, Inactive }
    public enum ChatbotTone { Formal, Informal, Friendly, Professional }
    public enum CompanyStatus { Active, Inactive, InReview }
    public enum ConversationStatus { Open, Closed, Pending }
    public enum RoleInConversation { User, Bot }
    public enum TypeOfDocument { pdf, docx, md, xlsx, txt }
}