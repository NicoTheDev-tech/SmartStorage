using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateDeliveryScheduleDto
    {
        public int BookingId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string GoodsDescription { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal EstimatedWeight { get; set; }
        public string SpecialInstructions { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
    }

    public class UpdateDeliveryScheduleDto
    {
        public int ScheduleId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }

    public class DeliveryScheduleResponseDto
    {
        public int Id { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;  // ← ADD THIS LINE
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string GoodsDescription { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal EstimatedWeight { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AssignedDriver { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public bool CanSchedule { get; set; }
        public bool CanReschedule { get; set; }
        public bool IsRated { get; set; }
        public int? Rating { get; set; } 
    }
}