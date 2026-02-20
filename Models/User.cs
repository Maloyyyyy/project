using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace JonyBalls3.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public string AvatarUrl { get; set; } = "";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }
        
        // Навигационные свойства (делаем nullable)
        public virtual ContractorProfile? ContractorProfile { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }
        public virtual ICollection<ChatMessage>? SentMessages { get; set; }
        public virtual ICollection<ChatMessage>? ReceivedMessages { get; set; }
        
        [NotMapped]
        public bool IsContractor => ContractorProfile != null;
    }
}