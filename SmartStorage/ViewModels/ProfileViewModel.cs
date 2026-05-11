using System.ComponentModel.DataAnnotations;

namespace SmartStorage.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Preferred name is required")]
        [Display(Name = "Preferred Name")]
        [StringLength(50, ErrorMessage = "Preferred name cannot exceed 50 characters")]
        public string PreferredName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;
    }
}