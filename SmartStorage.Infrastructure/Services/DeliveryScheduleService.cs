using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStorage.Infrastructure.Services
{
    public class DeliveryScheduleService : IDeliveryScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeliveryScheduleService> _logger;

        public DeliveryScheduleService(ApplicationDbContext context, ILogger<DeliveryScheduleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeliveryScheduleResponseDto> CreateSchedule(CreateDeliveryScheduleDto createDto, string userId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .FirstOrDefaultAsync(b => b.Id == createDto.BookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            var existingSchedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(s => s.BookingId == createDto.BookingId &&
                                         s.Status != ScheduleStatus.Cancelled);

            if (existingSchedule != null)
                throw new InvalidOperationException("A delivery schedule already exists for this booking");

            var isAvailable = await CheckTimeSlotAvailability(createDto.ScheduledDate, createDto.TimeSlot);
            if (!isAvailable)
                throw new InvalidOperationException("Selected time slot is not available");

            var deliveryType = Enum.Parse<DeliveryType>(createDto.DeliveryType);

            var schedule = new DeliverySchedule
            {
                ScheduleNumber = GenerateScheduleNumber(),
                BookingId = createDto.BookingId,
                ClientId = booking.ClientId,
                DeliveryType = deliveryType,
                ScheduledDate = createDto.ScheduledDate,
                ScheduledTime = ParseTimeSlot(createDto.TimeSlot),
                TimeSlot = createDto.TimeSlot,
                PickupAddress = createDto.PickupAddress,
                DeliveryAddress = createDto.DeliveryAddress,
                GoodsDescription = createDto.GoodsDescription,
                ItemCount = createDto.ItemCount,
                EstimatedWeight = createDto.EstimatedWeight,
                Status = ScheduleStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                SpecialInstructions = createDto.SpecialInstructions,
                ContactPerson = createDto.ContactPerson,
                ContactPhone = createDto.ContactPhone
            };

            _context.DeliverySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return await MapToDto(schedule);
        }

        public async Task<DeliveryScheduleResponseDto?> GetScheduleById(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(s => s.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .Include(s => s.AssignedDriver)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return null;

            return await MapToDto(schedule);
        }

        public async Task<IEnumerable<DeliveryScheduleResponseDto>> GetClientSchedules(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<DeliveryScheduleResponseDto>();

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
                return Enumerable.Empty<DeliveryScheduleResponseDto>();

            var schedules = await _context.DeliverySchedules
                .Include(s => s.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .Where(s => s.ClientId == client.Id)
                .OrderByDescending(s => s.ScheduledDate)
                .ToListAsync();

            var result = new List<DeliveryScheduleResponseDto>();
            foreach (var schedule in schedules)
            {
                result.Add(await MapToDto(schedule));
            }
            return result;
        }

        public async Task<IEnumerable<DeliveryScheduleResponseDto>> GetBookingSchedules(int bookingId)
        {
            var schedules = await _context.DeliverySchedules
                .Where(s => s.BookingId == bookingId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var result = new List<DeliveryScheduleResponseDto>();
            foreach (var schedule in schedules)
            {
                result.Add(await MapToDto(schedule));
            }
            return result;
        }

        public async Task<IEnumerable<DeliveryScheduleResponseDto>> GetPendingSchedules()
        {
            var schedules = await _context.DeliverySchedules
                .Include(s => s.Booking)
                .Where(s => s.Status == ScheduleStatus.Pending)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            var result = new List<DeliveryScheduleResponseDto>();
            foreach (var schedule in schedules)
            {
                result.Add(await MapToDto(schedule));
            }
            return result;
        }

        public async Task<DeliveryScheduleResponseDto> UpdateSchedule(UpdateDeliveryScheduleDto updateDto, string userId)
        {
            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(s => s.Id == updateDto.ScheduleId);

            if (schedule == null)
                throw new KeyNotFoundException("Schedule not found");

            if (schedule.Status != ScheduleStatus.Pending && schedule.Status != ScheduleStatus.Confirmed)
                throw new InvalidOperationException($"Cannot update schedule with status {schedule.Status}");

            if (updateDto.ScheduledDate.HasValue)
            {
                var timeSlot = updateDto.TimeSlot ?? schedule.TimeSlot;
                var isAvailable = await CheckTimeSlotAvailability(updateDto.ScheduledDate.Value, timeSlot);
                if (!isAvailable)
                    throw new InvalidOperationException("Selected time slot is not available");

                schedule.ScheduledDate = updateDto.ScheduledDate.Value;
                schedule.ScheduledTime = ParseTimeSlot(timeSlot);
                schedule.TimeSlot = timeSlot;
            }

            if (!string.IsNullOrEmpty(updateDto.TimeSlot))
            {
                var date = updateDto.ScheduledDate ?? schedule.ScheduledDate;
                var isAvailable = await CheckTimeSlotAvailability(date, updateDto.TimeSlot);
                if (!isAvailable)
                    throw new InvalidOperationException("Selected time slot is not available");

                schedule.TimeSlot = updateDto.TimeSlot;
                schedule.ScheduledTime = ParseTimeSlot(updateDto.TimeSlot);
            }

            schedule.SpecialInstructions = updateDto.SpecialInstructions ?? schedule.SpecialInstructions;
            schedule.ContactPerson = updateDto.ContactPerson ?? schedule.ContactPerson;
            schedule.ContactPhone = updateDto.ContactPhone ?? schedule.ContactPhone;

            await _context.SaveChangesAsync();

            return await MapToDto(schedule);
        }

        public async Task<DeliveryScheduleResponseDto> ConfirmSchedule(int scheduleId, string adminId)
        {
            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                throw new KeyNotFoundException("Schedule not found");

            if (schedule.Status != ScheduleStatus.Pending)
                throw new InvalidOperationException($"Cannot confirm schedule with status {schedule.Status}");

            schedule.Status = ScheduleStatus.Confirmed;
            schedule.ConfirmedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToDto(schedule);
        }

        public async Task<DeliveryScheduleResponseDto> CompleteSchedule(int scheduleId, string driverId)
        {
            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                throw new KeyNotFoundException("Schedule not found");

            if (schedule.Status != ScheduleStatus.Confirmed && schedule.Status != ScheduleStatus.InProgress)
                throw new InvalidOperationException($"Cannot complete schedule with status {schedule.Status}");

            schedule.Status = ScheduleStatus.Completed;
            schedule.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await MapToDto(schedule);
        }

        public async Task<DeliveryScheduleResponseDto> CancelSchedule(int scheduleId, string userId)
        {
            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                throw new KeyNotFoundException("Schedule not found");

            if (schedule.Status == ScheduleStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed schedule");

            schedule.Status = ScheduleStatus.Cancelled;

            await _context.SaveChangesAsync();

            return await MapToDto(schedule);
        }

        public async Task<bool> CheckTimeSlotAvailability(DateTime date, string timeSlot)
        {
            var existingSchedules = await _context.DeliverySchedules
                .Where(s => s.ScheduledDate.Date == date.Date &&
                           s.TimeSlot == timeSlot &&
                           s.Status != ScheduleStatus.Cancelled &&
                           s.Status != ScheduleStatus.Completed)
                .CountAsync();

            return existingSchedules < 3;
        }

        public async Task<IEnumerable<string>> GetAvailableTimeSlots(DateTime date)
        {
            var allTimeSlots = new[] { "08:00-10:00", "10:00-12:00", "12:00-14:00", "14:00-16:00", "16:00-18:00" };
            var availableSlots = new List<string>();

            foreach (var slot in allTimeSlots)
            {
                var isAvailable = await CheckTimeSlotAvailability(date, slot);
                if (isAvailable)
                {
                    availableSlots.Add(slot);
                }
            }

            return availableSlots;
        }

        private string GenerateScheduleNumber()
        {
            return $"SCH-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        private TimeSpan ParseTimeSlot(string timeSlot)
        {
            var parts = timeSlot.Split('-');
            if (parts.Length == 2 && TimeSpan.TryParse(parts[0], out var startTime))
            {
                return startTime;
            }
            return TimeSpan.FromHours(9);
        }

        private async Task<DeliveryScheduleResponseDto> MapToDto(DeliverySchedule schedule)
        {
            var booking = schedule.Booking;
            var storageUnit = booking?.StorageUnit;

            return await Task.Run(() => new DeliveryScheduleResponseDto
            {
                Id = schedule.Id,
                ScheduleNumber = schedule.ScheduleNumber ?? string.Empty,
                BookingId = schedule.BookingId,
                BookingNumber = booking?.BookingNumber ?? string.Empty,
                UnitNumber = storageUnit?.UnitNumber ?? "Unknown",
                DeliveryType = schedule.DeliveryType.ToString(),
                ScheduledDate = schedule.ScheduledDate,
                TimeSlot = schedule.TimeSlot,
                PickupAddress = schedule.PickupAddress,
                DeliveryAddress = schedule.DeliveryAddress,
                GoodsDescription = schedule.GoodsDescription,
                ItemCount = schedule.ItemCount,
                EstimatedWeight = schedule.EstimatedWeight,
                Status = schedule.Status.ToString(),
                CreatedAt = schedule.CreatedAt,
                ConfirmedAt = schedule.ConfirmedAt,
                AssignedDriver = schedule.AssignedDriver?.FullName,
                SpecialInstructions = schedule.SpecialInstructions,
                ContactPerson = schedule.ContactPerson,
                ContactPhone = schedule.ContactPhone,
                CanSchedule = schedule.Status == ScheduleStatus.Pending,
                CanReschedule = schedule.Status == ScheduleStatus.Pending || schedule.Status == ScheduleStatus.Confirmed
            });
        }
    }
}