using System.Collections.Generic;

namespace SmartStorage.ViewModels
{
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
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
        public List<RecentPaymentDto> RecentPayments { get; set; } = new();
    }

    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
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
}