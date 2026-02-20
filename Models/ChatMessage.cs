using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JonyBalls3.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        
        [Required]
        public string SenderId { get; set; }
        
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        
        [Required]
        public string ReceiverId { get; set; }
        
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
        
        [Required]
        public string Message { get; set; } = "";
        
        public DateTime SentAt { get; set; } = DateTime.Now;
        
        public DateTime? ReadAt { get; set; }
        
        public bool IsRead => ReadAt.HasValue;
        
        public string AttachmentUrl { get; set; } = "";
    }
}
