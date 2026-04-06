using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateCartageDto
    {
        public int BookingId { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? GoodsDescription { get; set; }
        public decimal GoodsWeight { get; set; }
        public int ItemCount { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}