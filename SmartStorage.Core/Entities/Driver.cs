using System;
using System.Collections.Generic;

namespace SmartStorage.Core.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public StaffRole Role { get; set; }
        public StaffStatus Status { get; set; }
        public DateTime HireDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string? LicenseNumber { get; set; }
        public string? VehicleAssigned { get; set; }
        public bool IsAvailable { get; set; }
        public DriverStatus DriverStatus { get; set; }
        public int? AssignedVehicleId { get; set; }

        // Navigation properties
        public ICollection<DeliverySchedule>? AssignedDeliveries { get; set; }
        public ICollection<Cartage>? Cartages { get; set; }
        public Vehicle? AssignedVehicle { get; set; }
    }

    public enum DriverStatus
    {
        Available = 0,
        OnDelivery = 1,
        Break = 2,
        OffDuty = 3
    }
}