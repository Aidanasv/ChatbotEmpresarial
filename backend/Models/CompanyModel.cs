namespace backend.Models
{
    public class Company
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string CIF { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CompanyStatus Status { get; set; } = CompanyStatus.InReview;
        public DateTime RenewalDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ChatbotSettings? ChatbotSettings { get; set; }
        public ICollection<Faq> Faqs { get; set; } = new List<Faq>();
        public ICollection<DocumentSource> DocumentSources { get; set; } = new List<DocumentSource>();

    }
}