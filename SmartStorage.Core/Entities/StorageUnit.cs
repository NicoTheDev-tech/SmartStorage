using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
        public class StorageUnit
        {
            public int Id { get; set; }
            public string? UnitNumber { get; set; }
            public string? Size { get; set; } // e.g., "10x10", "10x20"
            public decimal MonthlyRate { get; set; }
            public bool IsActive { get; set; }
            public string? Location { get; set; }
            public string? ClimateControl { get; set; } // None, Basic, Premium
            public ICollection<Booking>? Bookings { get; set; }
        }
}
