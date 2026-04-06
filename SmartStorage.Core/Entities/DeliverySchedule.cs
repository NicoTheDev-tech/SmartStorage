using System;

namespace SmartStorage.Core.Entities
{
    public class DeliverySchedule
    {
        public int Id { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public virtual Booking? Booking { get; set; }
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        // Delivery Details
        public DeliveryType DeliveryType { get; set; } // Pickup or Dropoff
        public DateTime ScheduledDate { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public string TimeSlot { get; set; } = string.Empty;

        // Address Details
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;

        // Goods Details
        public string GoodsDescription { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal EstimatedWeight { get; set; }

        // Status
        public ScheduleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Driver Assignment
        public int? AssignedDriverId { get; set; }
        public virtual Driver? AssignedDriver { get; set; }

        // Notes
        public string? SpecialInstructions { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
    }

    public enum DeliveryType
    {
        Pickup = 0,   // Customer picks up from facility
        Dropoff = 1,  // Customer drops off at facility
        Collection = 2 // SmartStorage collects from customer
    }

    public enum ScheduleStatus
    {
        Pending = 0,
        Confirmed = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Rescheduled = 5
    }
}