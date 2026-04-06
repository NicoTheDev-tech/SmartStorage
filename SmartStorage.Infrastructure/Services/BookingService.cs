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
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BookingResponseDto> CreateBooking(CreateBookingDto bookingDto, string userId)
        {
            if (bookingDto == null)
                throw new ArgumentNullException(nameof(bookingDto));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (bookingDto.ClientInfo == null)
            {
                bookingDto.ClientInfo = new ClientInfoDto
                {
                    FullName = "Customer",
                    Email = "customer@email.com",
                    Phone = "",
                    IdNumber = "",
                    Address = ""
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check availability - DISABLED FOR PRESENTATION
                var isAvailable = await CheckAvailability(bookingDto.StorageUnitId,
                    bookingDto.StartDate, bookingDto.EndDate);

                if (!isAvailable)
                    throw new InvalidOperationException("Storage unit not available for selected dates");

                // Get or create client with userId
                var client = await GetOrCreateClient(bookingDto.ClientInfo, userId);

                // Get storage unit
                var storageUnit = await _context.StorageUnits.FindAsync(bookingDto.StorageUnitId);

                if (storageUnit == null)
                    throw new KeyNotFoundException("Storage unit not found");

                // Calculate total amount - FIXED months calculation
                var startDate = bookingDto.StartDate;
                var endDate = bookingDto.EndDate;

                // Calculate exact months
                int months = ((endDate.Year - startDate.Year) * 12) + (endDate.Month - startDate.Month);
                if (months <= 0) months = 1;

                var totalAmount = (decimal)months * storageUnit.MonthlyRate;

                _logger.LogInformation($"Booking: Unit={storageUnit.UnitNumber}, MonthlyRate={storageUnit.MonthlyRate}, Months={months}, TotalAmount={totalAmount}");

                // Create booking
                var booking = new Booking
                {
                    BookingNumber = GenerateBookingNumber(),
                    ClientId = client.Id,
                    StorageUnitId = bookingDto.StorageUnitId,
                    StartDate = bookingDto.StartDate,
                    EndDate = bookingDto.EndDate,
                    TotalAmount = totalAmount,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create invoice for the booking
                var invoice = new Invoice
                {
                    InvoiceNumber = GenerateInvoiceNumber(),
                    BookingId = booking.Id,
                    ClientId = client.Id,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    Amount = totalAmount + 75,
                    AmountPaid = 0,
                    Balance = totalAmount + 75,
                    PeriodStart = booking.StartDate,
                    PeriodEnd = booking.EndDate,
                    BillingMonth = booking.StartDate.Month,
                    BillingYear = booking.StartDate.Year,
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Notes = $"Booking #{booking.BookingNumber}"
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var result = await GetBookingById(booking.Id);
                return result ?? throw new InvalidOperationException("Failed to retrieve created booking");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating booking for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckAvailability(int storageUnitId, DateTime startDate, DateTime endDate)
        {
            // TEMPORARY FIX FOR PRESENTATION - ALWAYS RETURN TRUE
            // This allows any booking regardless of date conflicts
            return await Task.FromResult(true);
        }

        private async Task<Client> GetOrCreateClient(ClientInfoDto clientInfo, string userId)
        {
            if (clientInfo == null)
                throw new ArgumentNullException(nameof(clientInfo));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            // First try to find client by UserId
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
            {
                // If not found by UserId, try by email or ID number
                client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == clientInfo.Email ||
                                              c.IdNumber == clientInfo.IdNumber);
            }

            if (client == null)
            {
                // Create new client
                client = new Client
                {
                    UserId = userId,
                    FullName = clientInfo.FullName ?? "Unknown",
                    Email = clientInfo.Email ?? "unknown@email.com",
                    Phone = clientInfo.Phone ?? string.Empty,
                    IdNumber = clientInfo.IdNumber ?? string.Empty,
                    Address = clientInfo.Address ?? string.Empty,
                    RegistrationDate = DateTime.UtcNow
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }
            else if (client.UserId != userId)
            {
                // Update existing client with UserId
                client.UserId = userId;
                client.FullName = clientInfo.FullName ?? client.FullName;
                client.Email = clientInfo.Email ?? client.Email;
                client.Phone = clientInfo.Phone ?? client.Phone;
                client.Address = clientInfo.Address ?? client.Address;
                client.IdNumber = clientInfo.IdNumber ?? client.IdNumber;

                await _context.SaveChangesAsync();
            }

            return client;
        }

        private string GenerateBookingNumber()
        {
            return $"BK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public async Task<BookingResponseDto?> GetBookingById(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return null;

            var amountPaid = booking.Payments?
                .Where(p => p.Status == PaymentStatus.Completed)
                .Sum(p => p.Amount) ?? 0;

            return new BookingResponseDto
            {
                Id = booking.Id,
                BookingNumber = booking.BookingNumber ?? string.Empty,
                ClientName = booking.Client?.FullName ?? string.Empty,
                ClientEmail = booking.Client?.Email ?? string.Empty,
                ClientPhone = booking.Client?.Phone ?? string.Empty,
                UnitNumber = booking.StorageUnit?.UnitNumber ?? string.Empty,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status.ToString(),
                AmountPaid = amountPaid,
                Balance = booking.TotalAmount - amountPaid
            };
        }

        public async Task<IEnumerable<BookingResponseDto>> GetClientBookings(int clientId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Include(b => b.Payments)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber ?? string.Empty,
                UnitNumber = b.StorageUnit?.UnitNumber ?? string.Empty,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                TotalAmount = b.TotalAmount,
                Status = b.Status.ToString(),
                AmountPaid = b.Payments?
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount) ?? 0,
                Balance = b.TotalAmount - (b.Payments?
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount) ?? 0)
            });
        }

        public async Task<IEnumerable<BookingResponseDto>> GetClientBookingsByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<BookingResponseDto>();

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
                return Enumerable.Empty<BookingResponseDto>();

            return await GetClientBookings(client.Id);
        }

        public async Task<BookingResponseDto> UpdateBookingStatus(int bookingId, string status)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (Enum.TryParse<BookingStatus>(status, true, out var newStatus))
            {
                booking.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            var updatedBooking = await GetBookingById(bookingId);
            return updatedBooking ?? throw new KeyNotFoundException("Booking not found after update");
        }

        public async Task<BookingResponseDto> CancelBooking(int bookingId, string userId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            // Verify ownership - check if the client matches the userId
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (booking.ClientId != client?.Id)
                throw new UnauthorizedAccessException("You can only cancel your own bookings");

            // Can only cancel pending or confirmed bookings that are NOT paid
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException($"Cannot cancel booking with status {booking.Status}");

            // Check if payment has been made
            var hasPayment = await _context.Payments.AnyAsync(p => p.BookingId == bookingId && p.Status == PaymentStatus.Completed);
            if (hasPayment)
                throw new InvalidOperationException("Cannot cancel a paid booking. Please contact support for refund requests.");

            booking.Status = BookingStatus.Cancelled;

            // Also cancel related invoice if exists and not paid
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
            if (invoice != null && invoice.Status != InvoiceStatus.Paid)
            {
                invoice.Status = InvoiceStatus.Cancelled;
            }

            // Cancel any pending delivery schedules
            var schedules = await _context.DeliverySchedules
                .Where(d => d.BookingId == bookingId && d.Status != ScheduleStatus.Completed)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                schedule.Status = ScheduleStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            var cancelledBooking = await GetBookingById(bookingId);
            if (cancelledBooking == null)
                throw new InvalidOperationException("Failed to retrieve cancelled booking");

            return cancelledBooking;
        }
    }
}