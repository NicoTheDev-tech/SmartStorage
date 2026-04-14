namespace SmartStorage.Core.DTOs
{
    public class ProcessPaymentDto
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
    }
}
