using System;

namespace SmartStorage.Core.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
        public string? UnitNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
    }
}