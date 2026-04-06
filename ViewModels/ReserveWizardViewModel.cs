using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage.ViewModels
{
    public class ReserveWizardViewModel
    {
        // Step 1: Storage Type
        [Required(ErrorMessage = "Please select a storage type")]
        public string StorageType { get; set; } = string.Empty;

        public List<StorageTypeOption> StorageTypes { get; set; } = new List<StorageTypeOption>();

        // Step 2: Filters
        public string Size { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool ClimateControlled { get; set; }
        public bool DriveUpAccess { get; set; }

        // Step 3: Selected Unit
        public int? SelectedUnitId { get; set; }
        public string SelectedUnitNumber { get; set; } = string.Empty;
        public string SelectedUnitSize { get; set; } = string.Empty;
        public decimal SelectedUnitPrice { get; set; }
        public string SelectedUnitFeatures { get; set; } = string.Empty;

        // Step 4: Duration
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

        public string Commitment { get; set; } = "Month-to-Month";
        public decimal DiscountPercentage { get; set; }

        // Step 5: Goods Details
        public string GoodsType { get; set; } = string.Empty;
        public int ItemCount { get; set; } = 1;
        public string SpecialInstructions { get; set; } = string.Empty;

        // Step 6: Cartage
        public bool NeedCartage { get; set; }
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime? PickupDate { get; set; }

        // Step 7: Vehicle Services
        public bool NeedVehicleService { get; set; }
        public string VehicleServiceType { get; set; } = string.Empty;

        // Step 8: Add-Ons
        public bool NeedMovingSupplies { get; set; }
        public bool NeedTruckRental { get; set; }
        public bool NeedMovingLabor { get; set; }
        public bool NeedInsurance { get; set; }
        public bool AutoPayDiscount { get; set; }

        // Calculated Values
        public decimal AdminFee => 25;
        public decimal SecurityDeposit => 50;
        public decimal InsuranceCost => NeedInsurance ? 15 : 0;
        public decimal MovingSuppliesCost => NeedMovingSupplies ? 32 : 0;
        public decimal TruckRentalCost => NeedTruckRental ? 30 : 0;
        public decimal MovingLaborCost => NeedMovingLabor ? 100 : 0;
        public int DurationMonths => (EndDate.Year - StartDate.Year) * 12 + (EndDate.Month - StartDate.Month);
        public decimal MonthlyRate => SelectedUnitPrice;
        public decimal DiscountedRate => MonthlyRate * (1 - (DiscountPercentage / 100));
        public decimal TotalMonthlyCost => (DiscountedRate > 0 ? DiscountedRate : MonthlyRate) + InsuranceCost;
        public decimal TotalDueToday => TotalMonthlyCost + AdminFee + SecurityDeposit + MovingSuppliesCost + TruckRentalCost + MovingLaborCost;
    }

    public class StorageTypeOption
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class AvailableUnit
    {
        public int Id { get; set; }
        public string UnitNumber { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string SizeCategory { get; set; } = string.Empty; // Small, Medium, Large, ExtraLarge
        public string Floor { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}