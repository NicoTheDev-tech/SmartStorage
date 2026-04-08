using SmartStorage.Core.DTOs;
using System.Collections.Generic;

namespace SmartStorage.ViewModels
{
    public class StaffDashboardViewModel
    {
        public int PendingIntakes { get; set; }
        public int PendingDeliveries { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedToday { get; set; }
        public int AvailableVehicles { get; set; }
        public int StorageUnitsInUse { get; set; }
        public List<DeliveryScheduleResponseDto> TodayDeliveries { get; set; } = new();
        public List<DeliveryScheduleResponseDto> PendingSchedules { get; set; } = new();
        public string? StaffName { get; set; }
    }
}