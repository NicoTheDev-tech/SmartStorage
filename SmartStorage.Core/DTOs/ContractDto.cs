using System;

namespace SmartStorage.Core.DTOs
{
    public class CreateContractDto
    {
        public int BookingId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal SecurityDeposit { get; set; }
        public string? SpecialConditions { get; set; }
    }

    public class ContractResponseDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string UnitNumber { get; set; } = string.Empty;
        public string UnitSize { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationMonths { get; set; }
        public decimal MonthlyRate { get; set; }
        public decimal SecurityDeposit { get; set; }
        public decimal TotalContractValue { get; set; }
        public string TermsAndConditions { get; set; } = string.Empty;
        public string SpecialConditions { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public bool CanAccept { get; set; }
        public bool CanActivate { get; set; }
    }

    public class AcceptContractDto
    {
        public int ContractId { get; set; }
        public bool AgreeToTerms { get; set; }
        public string? AcceptedBy { get; set; }
    }
}