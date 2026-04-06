using System;

namespace SmartStorage.Core.Entities
{
    public class Contract
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public virtual Booking? Booking { get; set; }
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        // Contract Details
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal TotalContractValue { get; set; }

        // Terms
        public string TermsAndConditions { get; set; } = string.Empty;
        public string SpecialConditions { get; set; } = string.Empty;

        // Status
        public ContractStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }

        // Acceptance
        public string? AcceptedBy { get; set; }
        public string? AcceptedIpAddress { get; set; }
    }

    public enum ContractStatus
    {
        Draft = 0,
        PendingAcceptance = 1,
        Accepted = 2,
        Active = 3,
        Expired = 4,
        Terminated = 5,
        Cancelled = 6
    }
}