using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Cartage
    {
        public int Id { get; set; }
        public string? CartageNumber { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int? DriverId { get; set; }
        public Driver? Driver { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? GoodsDescription { get; set; }
        public decimal GoodsWeight { get; set; } // in kg
        public int ItemCount { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public CartageStatus Status { get; set; }
        public decimal Cost { get; set; }
    }

    public enum CartageStatus
    {
        Scheduled,
        PendingPickup,
        InTransit,
        Delivered,
        Cancelled
    }
}
