using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? ServiceType { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public string? ServiceProvider { get; set; }
        public MaintenanceStatus Status { get; set; }
    }

    public enum MaintenanceStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }
}
