using System;

namespace SmartStorage.Core.DTOs
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public bool IsOverdue => DateTime.UtcNow > DueDate && Status != "Paid";
        public bool CanPay => Status != "Paid" && Status != "Cancelled";
    }

    public class CreateInvoiceDto
    {
        public int ContractId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    public class ProcessPaymentDto
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string? TransactionId { get; set; }
        public string? ProofOfPaymentPath { get; set; }
    }

    public class PaymentConfirmationDto
    {
        public int PaymentId { get; set; }
        public bool IsVerified { get; set; }
        public string? VerifiedBy { get; set; }
        public string? Notes { get; set; }
    }
}