using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ChatbotSettings> ChatbotSettings { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<DocumentSource> DocumentSources { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Company>()
                .Property(c => c.Status)
                .HasConversion<string>();


            modelBuilder.Entity<Subscription>()
                .HasData(
                    new Subscription { Id = 1, Name = "Basic", Price = 9.99m, MaxUsers = 2, Features = "Acceso al chatbot, Soporte básico" },
                    new Subscription { Id = 2, Name = "Standard", Price = 19.99m, MaxUsers = 5, Features = "Acceso al chatbot, Soporte estándar" },
                    new Subscription { Id = 3, Name = "Premium", Price = 29.99m, MaxUsers = 8, Features = "Acceso al chatbot, Soporte premium" }
                );

            modelBuilder.Entity<ChatbotSettings>()
                .Property(c => c.ChatbotTone)
                .HasConversion<string>();

            modelBuilder.Entity<ChatbotSettings>()
                .Property(c => c.ShowAvatar)
                .HasConversion<string>();

            modelBuilder.Entity<Conversation>()
                .Property(c => c.Status)
                .HasConversion<string>();
            
            modelBuilder.Entity<Message>()
                .Property(m => m.Role)
                .HasConversion<string>();

            modelBuilder.Entity<DocumentSource>()
                .Property(d => d.DocumentType)
                .HasConversion<string>();
        }
    }
}