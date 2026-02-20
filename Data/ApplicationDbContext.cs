using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using JonyBalls3.Models;

namespace JonyBalls3.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ContractorProfile> ContractorProfiles { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectStage> ProjectStages { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Настройка связей
            modelBuilder.Entity<ContractorProfile>()
                .HasOne(c => c.User)
                .WithOne(u => u.ContractorProfile)
                .HasForeignKey<ContractorProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Contractor)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ContractorId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Индексы
            modelBuilder.Entity<ContractorProfile>()
                .HasIndex(c => c.Specialization);
            
            modelBuilder.Entity<ContractorProfile>()
                .HasIndex(c => c.Rating);
            
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.UserId);
            
            modelBuilder.Entity<Project>()
                .HasIndex(p => p.ContractorId);
            
            modelBuilder.Entity<ChatMessage>()
                .HasIndex(m => m.ProjectId);
        }
    }
}
