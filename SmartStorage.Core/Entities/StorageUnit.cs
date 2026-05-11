using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorage.Core.Entities
{
    public class StorageUnit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string UnitNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Size { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ClimateControl { get; set; } = "None";

        public string? Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Booking>? Bookings { get; set; }

        // Computed property - not stored in database
        [NotMapped]
        public OccupancyStatus OccupancyStatus
        {
            get
            {
                if (!IsActive)
                    return OccupancyStatus.Maintenance;

                if (Bookings != null && Bookings.Any(b =>
                    b.Status == BookingStatus.Confirmed ||
                    b.Status == BookingStatus.Active))
                {
                    return OccupancyStatus.Occupied;
                }

                if (Bookings != null && Bookings.Any(b => b.Status == BookingStatus.Pending))
                {
                    return OccupancyStatus.Reserved;
                }

                return OccupancyStatus.Available;
            }
        }
    }

    public enum OccupancyStatus
    {
        Available = 0,
        Occupied = 1,
        Reserved = 2,
        Maintenance = 3
    }
}