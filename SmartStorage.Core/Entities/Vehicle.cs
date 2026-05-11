using System;

namespace SmartStorage.Core.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Model { get; set; }
        public string? Type { get; set; }
        public decimal Capacity { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public VehicleStatus Status { get; set; }

        // Navigation properties
        public Driver? Driver { get; set; }
    }

    public enum VehicleStatus
    {
        Available = 0,
        InUse = 1,
        Maintenance = 2,
        Retired = 3
    }
}