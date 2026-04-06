using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IVehicleService
    {
        Task<Vehicle> AddVehicle(Vehicle vehicle);
        Task<MaintenanceRecord> ScheduleMaintenance(CreateMaintenanceDto maintenanceDto);
        Task<MaintenanceRecord> CompleteMaintenance(int maintenanceId);
        Task<IEnumerable<Vehicle>> GetVehiclesDueForMaintenance();
    }
}
