using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Model { get; set; }
        public string? Type { get; set; } // Truck, Van, etc.
        public decimal Capacity { get; set; } // in kg
        public DateTime PurchaseDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public VehicleStatus Status { get; set; }
        public ICollection<MaintenanceRecord>? MaintenanceRecords { get; set; }
    }

    public enum VehicleStatus
    {
        Available,
        InUse,
        UnderMaintenance,
        OutOfService
    }
}
