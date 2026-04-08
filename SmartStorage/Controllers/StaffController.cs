using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using SmartStorage.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "Staff")]
    [Route("Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeliveryScheduleService _deliveryService;
        private readonly UserManager<IdentityUser> _userManager;

        public StaffController(
            ApplicationDbContext context,
            IDeliveryScheduleService deliveryService,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _deliveryService = deliveryService;
            _userManager = userManager;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get today's deliveries
            var todayDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Where(d => d.ScheduledDate.Date == today && d.Status != ScheduleStatus.Completed)
                .OrderBy(d => d.ScheduledTime)
                .ToListAsync();

            // Get pending schedules
            var pendingSchedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Where(d => d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .Take(10)
                .ToListAsync();

            var viewModel = new StaffDashboardViewModel
            {
                PendingIntakes = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending && d.DeliveryType == DeliveryType.Dropoff),
                PendingDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending && d.DeliveryType == DeliveryType.Collection),
                InProgressTasks = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Confirmed),
                CompletedToday = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Completed && d.CompletedAt.HasValue && d.CompletedAt.Value.Date == today),
                AvailableVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Available),
                StorageUnitsInUse = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Active),
                TodayDeliveries = todayDeliveries.Select(d => MapToDtoStatic(d)).ToList(),
                PendingSchedules = pendingSchedules.Select(d => MapToDtoStatic(d)).ToList()
            };

            // Get staff name
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                viewModel.StaffName = user?.UserName?.Split('@')[0] ?? "Staff";
            }
            else
            {
                viewModel.StaffName = "Staff";
            }

            return View(viewModel);
        }

        [HttpGet("GoodsIntake")]
        public async Task<IActionResult> GoodsIntake()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .Where(d => d.Status == ScheduleStatus.Pending && d.DeliveryType == DeliveryType.Dropoff)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();
            return View(schedules);
        }

        [HttpPost("GoodsIntake/Start/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartIntake(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.Pending)
            {
                schedule.Status = ScheduleStatus.InProgress;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Goods intake started";
            }
            return RedirectToAction("GoodsIntake");
        }

        [HttpPost("GoodsIntake/Complete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteIntake(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (schedule != null && schedule.Status == ScheduleStatus.InProgress)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.CompletedAt = DateTime.Now;

                if (schedule.Booking != null && schedule.Booking.Status == BookingStatus.Pending)
                {
                    schedule.Booking.Status = BookingStatus.Confirmed;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Goods intake completed and stored";
            }
            return RedirectToAction("GoodsIntake");
        }

        [HttpGet("StorageAllocation")]
        public async Task<IActionResult> StorageAllocation()
        {
            var units = await _context.StorageUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.UnitNumber)
                .ToListAsync();
            return View(units);
        }

        [HttpGet("StorageOperations")]
        public async Task<IActionResult> StorageOperations()
        {
            var activeBookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .Where(b => b.Status == BookingStatus.Active)
                .OrderBy(b => b.EndDate)
                .ToListAsync();
            return View(activeBookings);
        }

        [HttpGet("DeliveryHandling")]
        public async Task<IActionResult> DeliveryHandling()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(d => d.AssignedDriver)
                .Where(d => d.Status == ScheduleStatus.Pending || d.Status == ScheduleStatus.Confirmed)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();
            return View(schedules);
        }

        [HttpPost("DeliveryHandling/Assign/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToSelf(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (schedule != null && schedule.Status == ScheduleStatus.Pending && !string.IsNullOrEmpty(userId))
            {
                // Ensure driver record exists
                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
                if (driver == null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    driver = new Driver
                    {
                        UserId = userId,
                        FullName = user?.UserName ?? "Staff",
                        LicenseNumber = "PENDING",
                        Phone = "",
                        IsAvailable = true
                    };
                    _context.Drivers.Add(driver);
                    await _context.SaveChangesAsync();
                }

                schedule.Status = ScheduleStatus.Confirmed;
                schedule.AssignedDriverId = driver.Id;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery assigned to you";
            }
            return RedirectToAction("DeliveryHandling");
        }

        [HttpPost("DeliveryHandling/Complete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteDelivery(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.Confirmed)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery completed successfully";
            }
            return RedirectToAction("DeliveryHandling");
        }

        [HttpGet("Tasks")]
        public async Task<IActionResult> Tasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tasks = new List<DeliverySchedule>();

            if (!string.IsNullOrEmpty(userId))
            {
                // First, find the staff member as a Driver
                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

                if (driver != null)
                {
                    tasks = await _context.DeliverySchedules
                        .Include(d => d.Booking)
                            .ThenInclude(b => b != null ? b.Client : null)
                        .Include(d => d.Booking)
                            .ThenInclude(b => b != null ? b.StorageUnit : null)
                        .Where(d => d.AssignedDriverId == driver.Id && d.Status != ScheduleStatus.Completed)
                        .OrderBy(d => d.ScheduledDate)
                        .ToListAsync();
                }
                else
                {
                    // If no driver record exists, create one automatically
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        driver = new Driver
                        {
                            UserId = userId,
                            FullName = user.UserName ?? "Staff",
                            LicenseNumber = "PENDING",
                            Phone = string.Empty,
                            IsAvailable = true
                        };
                        _context.Drivers.Add(driver);
                        await _context.SaveChangesAsync();

                        // Now get tasks assigned to this new driver
                        tasks = await _context.DeliverySchedules
                            .Include(d => d.Booking)
                                .ThenInclude(b => b != null ? b.Client : null)
                            .Include(d => d.Booking)
                                .ThenInclude(b => b != null ? b.StorageUnit : null)
                            .Where(d => d.AssignedDriverId == driver.Id && d.Status != ScheduleStatus.Completed)
                            .OrderBy(d => d.ScheduledDate)
                            .ToListAsync();
                    }
                }
            }

            return View(tasks);
        }

        private async Task<int?> GetOrCreateDriverId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    driver = new Driver
                    {
                        UserId = userId,
                        FullName = user.UserName ?? "Staff",
                        LicenseNumber = "PENDING",
                        Phone = string.Empty,
                        IsAvailable = true
                    };
                    _context.Drivers.Add(driver);
                    await _context.SaveChangesAsync();
                }
            }
            return driver?.Id;
        }

        // Static method to map DeliverySchedule to DTO - FIXES THE ERROR
        private static DeliveryScheduleResponseDto MapToDtoStatic(DeliverySchedule schedule)
        {
            return new DeliveryScheduleResponseDto
            {
                Id = schedule.Id,
                ScheduleNumber = schedule.ScheduleNumber ?? string.Empty,
                BookingId = schedule.BookingId,
                BookingNumber = schedule.Booking?.BookingNumber ?? string.Empty,
                UnitNumber = schedule.Booking?.StorageUnit?.UnitNumber ?? "Unknown",
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
                ContactPhone = schedule.ContactPhone
            };
        }
    }
}