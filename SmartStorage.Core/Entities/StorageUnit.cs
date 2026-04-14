using System;
using System.Collections.Generic;

namespace SmartStorage.Core.Entities
{
    public class StorageUnit
    {
        public int Id { get; set; }
        public string? UnitNumber { get; set; }      // Changed from Name
        public string? Size { get; set; }
        public decimal MonthlyRate { get; set; }     // Changed from Price
        public bool IsActive { get; set; }           // Changed from IsAvailable
        public string? Location { get; set; }
        public string? ClimateControl { get; set; }

        // Additional properties if needed
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Features { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}