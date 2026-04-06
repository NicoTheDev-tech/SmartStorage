using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public string? PaymentReference { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ProofOfPaymentPath { get; set; }
        public string? TransactionId { get; set; } // For online payments
    }

    public enum PaymentMethod
    {
        Cash,
        BankTransfer,
        CreditCard,
        MobileMoney
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
}
