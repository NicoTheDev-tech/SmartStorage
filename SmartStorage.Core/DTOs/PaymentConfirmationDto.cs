using System;

namespace SmartStorage.Core.DTOs
{
    public class PaymentConfirmationDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
    }
}