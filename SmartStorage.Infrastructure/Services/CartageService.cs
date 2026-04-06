using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;  // Add this for CreateCartageDto
using SmartStorage.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStorage.Infrastructure.Services
{
    public class CartageService : ICartageService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartageService> _logger;

        public CartageService(ApplicationDbContext context, ILogger<CartageService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cartage> CreateCartage(CreateCartageDto cartageDto)
        {
            var booking = await _context.Bookings.FindAsync(cartageDto.BookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            var cartage = new Cartage
            {
                CartageNumber = GenerateCartageNumber(),
                BookingId = cartageDto.BookingId,
                PickupAddress = cartageDto.PickupAddress,
                DeliveryAddress = cartageDto.DeliveryAddress,
                GoodsDescription = cartageDto.GoodsDescription,
                GoodsWeight = cartageDto.GoodsWeight,
                ItemCount = cartageDto.ItemCount,
                ScheduledDate = cartageDto.ScheduledDate,
                Status = CartageStatus.Scheduled,
                Cost = CalculateCartageCost(cartageDto.GoodsWeight, cartageDto.ItemCount)
            };

            _context.Cartages.Add(cartage);
            await _context.SaveChangesAsync();

            return cartage;
        }

        public async Task<Cartage> AssignDriver(int cartageId, int driverId)
        {
            var cartage = await _context.Cartages.FindAsync(cartageId);
            var driver = await _context.Drivers
                .Include(d => d.AssignedVehicle)
                .FirstOrDefaultAsync(d => d.Id == driverId && d.IsAvailable);

            if (cartage == null || driver == null)
                throw new KeyNotFoundException("Cartage or driver not found");

            cartage.DriverId = driverId;
            cartage.Status = CartageStatus.PendingPickup;
            driver.IsAvailable = false;

            await _context.SaveChangesAsync();

            // Send notification to driver
            await NotifyDriver(driver, cartage);

            return cartage;
        }

        public async Task<Cartage> UpdateCartageStatus(int cartageId, string status)
        {
            var cartage = await _context.Cartages
                .Include(c => c.Driver)
                .FirstOrDefaultAsync(c => c.Id == cartageId);

            if (cartage == null)
                throw new KeyNotFoundException("Cartage not found");

            if (Enum.TryParse<CartageStatus>(status, true, out var newStatus))
            {
                cartage.Status = newStatus;

                // Update timestamps based on status
                if (newStatus == CartageStatus.InTransit)
                    cartage.PickupDate = DateTime.UtcNow;
                else if (newStatus == CartageStatus.Delivered)
                {
                    cartage.DeliveryDate = DateTime.UtcNow;
                    if (cartage.Driver != null)
                        cartage.Driver.IsAvailable = true;
                }

                await _context.SaveChangesAsync();
            }

            return cartage;
        }

        public async Task<IEnumerable<Cartage>> GetDriverCartages(int driverId)
        {
            return await _context.Cartages
                .Where(c => c.DriverId == driverId)
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();
        }

        private string GenerateCartageNumber()
        {
            return $"CTG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }

        private decimal CalculateCartageCost(decimal weight, int itemCount)
        {
            // Simple cost calculation - can be made more sophisticated
            decimal baseRate = 50;
            decimal weightRate = weight * 2;
            decimal itemRate = itemCount * 5;

            return baseRate + weightRate + itemRate;
        }

        private async Task NotifyDriver(Driver driver, Cartage cartage)
        {
            // Implement notification logic (SMS, Email, Push notification)
            _logger.LogInformation($"Notification sent to driver {driver.FullName} for cartage {cartage.CartageNumber}");
            await Task.CompletedTask;
        }
    }
}
