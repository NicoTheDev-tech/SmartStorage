using SmartStorage.Core.DTOs;  // Add this line
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBooking(CreateBookingDto bookingDto, string userId);
        Task<bool> CheckAvailability(int storageUnitId, DateTime startDate, DateTime endDate);
        Task<BookingResponseDto?> GetBookingById(int id);
        Task<IEnumerable<BookingResponseDto>> GetClientBookings(int clientId);
        Task<IEnumerable<BookingResponseDto>> GetClientBookingsByUserId(string userId);
        Task<BookingResponseDto> UpdateBookingStatus(int bookingId, string status);
        Task<BookingResponseDto> CancelBooking(int bookingId, string userId);
    }
}