using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStorage.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            ApplicationDbContext context,
            IHostEnvironment environment,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<Payment> ProcessPayment(CreatePaymentDto paymentDto)
        {
            if (paymentDto == null)
                throw new ArgumentNullException(nameof(paymentDto));

            var booking = await _context.Bookings.FindAsync(paymentDto.BookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            string? proofPath = null;
            if (paymentDto.ProofOfPayment != null)
            {
                proofPath = await SaveProofOfPayment(paymentDto.ProofOfPayment);
            }

            if (string.IsNullOrEmpty(paymentDto.PaymentMethod))
                throw new ArgumentException("Payment method is required");

            if (!Enum.TryParse<PaymentMethod>(paymentDto.PaymentMethod, true, out var paymentMethod))
            {
                throw new ArgumentException($"Invalid payment method: {paymentDto.PaymentMethod}");
            }

            var payment = new Payment
            {
                PaymentReference = GeneratePaymentReference(),
                BookingId = paymentDto.BookingId,
                Amount = paymentDto.Amount,
                PaymentDate = DateTime.UtcNow,
                Method = paymentMethod,
                Status = PaymentStatus.Pending,
                ProofOfPaymentPath = proofPath
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await UpdateBookingPaymentStatus(booking.Id);

            return payment;
        }

        public async Task<InvoiceDto> GenerateInvoice(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (booking.Client == null)
                throw new InvalidOperationException("Booking client information is missing");

            if (booking.StorageUnit == null)
                throw new InvalidOperationException("Booking storage unit information is missing");

            var months = (int)Math.Ceiling((booking.EndDate - booking.StartDate).Days / 30.0);
            var totalAmount = booking.StorageUnit.MonthlyRate * months;

            var invoice = new InvoiceDto
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{bookingId}",
                InvoiceDate = DateTime.Now,
                ContractNumber = $"CTR-{bookingId}",
                ClientName = booking.Client.FullName ?? "Unknown",
                ClientEmail = booking.Client.Email ?? "Unknown",
                UnitNumber = booking.StorageUnit.UnitNumber ?? "Unknown",
                Amount = totalAmount,
                AmountPaid = booking.Payments?.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount) ?? 0,
                Balance = totalAmount - (booking.Payments?.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount) ?? 0),
                PeriodStart = booking.StartDate,
                PeriodEnd = booking.EndDate,
                Status = totalAmount <= (booking.Payments?.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount) ?? 0) ? "Paid" : "Pending",
                DueDate = booking.StartDate.AddDays(7)
            };

            return invoice;
        }

        private async Task<string> SaveProofOfPayment(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "payments");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Path.Combine("uploads", "payments", uniqueFileName);
        }

        private string GeneratePaymentReference()
        {
            return $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private async Task UpdateBookingPaymentStatus(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking != null)
            {
                var totalPaid = booking.Payments?
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount) ?? 0;

                var months = (int)Math.Ceiling((booking.EndDate - booking.StartDate).Days / 30.0);
                var storageUnit = await _context.StorageUnits.FindAsync(booking.StorageUnitId);
                var totalAmount = (storageUnit?.MonthlyRate ?? 0) * months;

                if (totalPaid >= totalAmount && booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Confirmed;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<Payment>> GetBookingPayments(int bookingId)
        {
            return await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<bool> VerifyPayment(string paymentReference)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentReference == paymentReference);

            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Completed;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}