using System.ComponentModel.DataAnnotations;
using SmartStorage.Core.Entities;

namespace SmartStorage.ViewModels
{
    public class ReserveViewModel
    {
        public List<StorageUnit> AvailableUnits { get; set; } = new();

        // Step 1 - Unit Selection
        public int? SelectedUnitId { get; set; }
        public string? SelectedUnitNumber { get; set; }
        public decimal? SelectedUnitRate { get; set; }

        // Step 2 - Date Selection
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

        // Step 3 - Client Information
        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID/Passport number is required")]
        [Display(Name = "ID/Passport Number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        // Calculated fields
        public int DurationDays => (EndDate - StartDate).Days;
        public int DurationMonths => (int)Math.Ceiling(DurationDays / 30.0);
        public decimal TotalAmount => SelectedUnitRate.HasValue ? SelectedUnitRate.Value * DurationMonths : 0;
        public bool IsUnitSelected => SelectedUnitId.HasValue;
        public bool AreDatesValid => StartDate >= DateTime.Today && EndDate > StartDate;
    }
}