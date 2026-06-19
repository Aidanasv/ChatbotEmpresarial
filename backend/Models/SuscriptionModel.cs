namespace backend.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public string Features { get; set; } = string.Empty;
        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}