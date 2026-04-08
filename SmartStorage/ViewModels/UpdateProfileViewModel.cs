using System.ComponentModel.DataAnnotations;

namespace SmartStorage.ViewModels
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Preferred name is required")]
        [Display(Name = "Preferred Name")]
        public string PreferredName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID/Passport number is required")]
        [Display(Name = "ID/Passport Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Physical Address")]
        public string Address { get; set; } = string.Empty;
    }
}