using System;

namespace SmartStorage.Core.Entities
{
    public class ContractExtension
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int RequestedDays { get; set; }
        public DateTime RequestedDate { get; set; } = DateTime.Now;
        public DateTime CurrentEndDate { get; set; }
        public DateTime ProposedNewEndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? AdminNotes { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }

        // Navigation property
        public Contract? Contract { get; set; }
    }
}