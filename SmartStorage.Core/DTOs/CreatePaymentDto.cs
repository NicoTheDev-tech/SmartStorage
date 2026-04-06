using Microsoft.AspNetCore.Http;

namespace SmartStorage.Core.DTOs
{
    public class CreatePaymentDto
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public IFormFile? ProofOfPayment { get; set; }
    }
}