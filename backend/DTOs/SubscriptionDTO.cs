namespace backend.DTOs
{
    public class SubscriptionAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public List<string> Features { get; set; } = new();
        public int CompaniesCount { get; set; }
        public decimal ProjectedMonthlyRevenue { get; set; }
    }

    public class UpsertSubscriptionDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public List<string> Features { get; set; } = new();
    }
}
