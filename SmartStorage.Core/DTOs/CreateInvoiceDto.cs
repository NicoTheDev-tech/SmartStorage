using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateInvoiceDto
    {
        public int BookingId { get; set; }
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
