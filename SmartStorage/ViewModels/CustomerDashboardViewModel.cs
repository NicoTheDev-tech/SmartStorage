using SmartStorage.Core.DTOs;
using System.Collections.Generic;

namespace SmartStorage.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public int ActiveBookingsCount { get; set; }
        public int ActiveContractsCount { get; set; }
        public int PendingInvoicesCount { get; set; }
        public int UpcomingDeliveriesCount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public IEnumerable<BookingResponseDto> RecentBookings { get; set; } = new List<BookingResponseDto>();
        public IEnumerable<ContractResponseDto> RecentContracts { get; set; } = new List<ContractResponseDto>();
        public IEnumerable<InvoiceDto> RecentInvoices { get; set; } = new List<InvoiceDto>();
        public string? ClientName { get; set; }
        public string? ClientPreferredName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }
    }
}