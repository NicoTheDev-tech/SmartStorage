using System;

namespace SmartStorage.Core.Entities
{
    public class BreachCase
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string BreachReason { get; set; } = string.Empty;
        public DateTime BreachDate { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Closed
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}