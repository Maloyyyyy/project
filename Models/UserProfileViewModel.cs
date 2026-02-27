using System.ComponentModel.DataAnnotations;

namespace JonyBalls3.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Display(Name = "Телефон")]
        [Phone]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "О себе")]
        [StringLength(500)]
        public string? AboutMe { get; set; }
        
        [Display(Name = "Город")]
        [StringLength(100)]
        public string? City { get; set; }
        
        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        
        [Display(Name = "Аватар")]
        public string? AvatarUrl { get; set; }
        
        public IFormFile? AvatarFile { get; set; }
        
        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; set; }
        
        public int ProjectsCount { get; set; }
        public int CompletedProjectsCount { get; set; }
    }
}