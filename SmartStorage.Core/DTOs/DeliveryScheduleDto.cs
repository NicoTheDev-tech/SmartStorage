using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateDeliveryScheduleDto
    {
        public int BookingId { get; set; }
        public string DeliveryType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string GoodsDescription { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal EstimatedWeight { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
    }

    public class DeliveryScheduleResponseDto
    {
        public int Id { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string DeliveryType { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string GoodsDescription { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal EstimatedWeight { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? AssignedDriver { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public bool CanSchedule { get; set; }
        public bool CanReschedule { get; set; }
    }

    public class UpdateDeliveryScheduleDto
    {
        public int ScheduleId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? TimeSlot { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
    }
}