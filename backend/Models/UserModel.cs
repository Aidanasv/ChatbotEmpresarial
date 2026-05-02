namespace backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public Company? Company { get; set; } = null!;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.User;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}