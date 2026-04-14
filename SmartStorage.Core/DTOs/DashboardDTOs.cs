using System;
using System.Collections.Generic;

namespace SmartStorage.Core.DTOs
{
    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecentPaymentDto
    {
        public int Id { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ActiveContracts { get; set; }
        public int PendingContracts { get; set; }
        public int TotalStorageUnits { get; set; }
        public int AvailableUnits { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingDeliveries { get; set; }
        public List<RecentBookingDto> RecentBookings { get; set; } = new List<RecentBookingDto>();
        public List<RecentPaymentDto> RecentPayments { get; set; } = new List<RecentPaymentDto>();
    }

    public class RecentInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class CustomerDashboardViewModel
    {
        public int ActiveBookings { get; set; }
        public int ActiveContracts { get; set; }
        public int TotalInvoices { get; set; }
        public int UpcomingDeliveries { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public IEnumerable<RecentBookingDto> RecentBookings { get; set; } = new List<RecentBookingDto>();
        public IEnumerable<RecentInvoiceDto> RecentInvoices { get; set; } = new List<RecentInvoiceDto>();
        public string ClientName { get; set; } = string.Empty;
        public string ClientPreferredName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
    }

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int? ContractId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusValue { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
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
    }

}
