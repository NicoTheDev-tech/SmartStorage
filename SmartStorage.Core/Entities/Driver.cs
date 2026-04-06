using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? FullName { get; set; }
        public string? LicenseNumber { get; set; }
        public string? Phone { get; set; }
        public bool IsAvailable { get; set; }
        public int? AssignedVehicleId { get; set; }
        public Vehicle? AssignedVehicle { get; set; }
        public ICollection<Cartage>? Cartages { get; set; }
    }
}
