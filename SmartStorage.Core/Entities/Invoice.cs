using System;

namespace SmartStorage.Core.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public int? ContractId { get; set; }
        public int ClientId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int BillingMonth { get; set; }
        public int BillingYear { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Booking? Booking { get; set; }
        public virtual Client? Client { get; set; }
    }

    public enum InvoiceStatus
    {
        Pending = 0,
        Paid = 1,
        Overdue = 2,
        Cancelled = 3,
        PartiallyPaid = 4
    }
}