using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.DTOs
{
    public class CreateMaintenanceDto
    {
        public int VehicleId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? ServiceType { get; set; }
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public string? ServiceProvider { get; set; }
    }
}
