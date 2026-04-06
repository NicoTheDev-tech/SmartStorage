using SmartStorage.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IDeliveryScheduleService
    {
        Task<DeliveryScheduleResponseDto> CreateSchedule(CreateDeliveryScheduleDto createDto, string userId);
        Task<DeliveryScheduleResponseDto?> GetScheduleById(int id);
        Task<IEnumerable<DeliveryScheduleResponseDto>> GetClientSchedules(string userId);
        Task<IEnumerable<DeliveryScheduleResponseDto>> GetBookingSchedules(int bookingId);
        Task<IEnumerable<DeliveryScheduleResponseDto>> GetPendingSchedules();
        Task<DeliveryScheduleResponseDto> UpdateSchedule(UpdateDeliveryScheduleDto updateDto, string userId);
        Task<DeliveryScheduleResponseDto> ConfirmSchedule(int scheduleId, string adminId);
        Task<DeliveryScheduleResponseDto> CompleteSchedule(int scheduleId, string driverId);
        Task<DeliveryScheduleResponseDto> CancelSchedule(int scheduleId, string userId);
        Task<bool> CheckTimeSlotAvailability(DateTime date, string timeSlot);
        Task<IEnumerable<string>> GetAvailableTimeSlots(DateTime date);
    }
}