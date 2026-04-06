using SmartStorage.Core.DTOs;

namespace SmartStorage.ViewModels
{
    public class BookingViewModel
    {
        public BookingResponseDto? Booking { get; set; }
        public bool CanScheduleDelivery { get; set; }
    }
}