using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "OperationsManager")]
    [Route("Operations")]
    public class OperationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OperationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var pendingDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            var confirmedDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Where(d => d.Status == ScheduleStatus.Confirmed)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            var completedDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Where(d => d.Status == ScheduleStatus.Completed)
                .OrderByDescending(d => d.CompletedAt)
                .Take(10)
                .ToListAsync();

            // Safe query with null check
            var availableDrivers = new List<Driver>();
            try
            {
                availableDrivers = await _context.Drivers
                    .Where(d => d.IsAvailable == true)
                    .ToListAsync();
            }
            catch { }

            var totalDeliveries = await _context.DeliverySchedules.CountAsync();
            var completedCount = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Completed);
            var onTimeRate = totalDeliveries > 0 ? (completedCount * 100 / totalDeliveries) : 0;

            ViewBag.PendingDeliveries = pendingDeliveries;
            ViewBag.ConfirmedDeliveries = confirmedDeliveries;
            ViewBag.CompletedDeliveries = completedDeliveries;
            ViewBag.AvailableDrivers = availableDrivers;
            ViewBag.TotalDeliveries = totalDeliveries;
            ViewBag.CompletedCount = completedCount;
            ViewBag.OnTimeRate = onTimeRate;

            return View();
        }

        [HttpGet("AssignDrivers")]
        public async Task<IActionResult> AssignDrivers()
        {
            var pendingDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            var drivers = new List<Driver>();
            try
            {
                drivers = await _context.Drivers.ToListAsync();
            }
            catch { }

            ViewBag.PendingDeliveries = pendingDeliveries;
            ViewBag.Drivers = drivers;

            return View();
        }

        [HttpPost("AssignDriver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int deliveryId, int driverId)
        {
            var delivery = await _context.DeliverySchedules.FindAsync(deliveryId);
            var driver = await _context.Drivers.FindAsync(driverId);

            if (delivery != null && driver != null)
            {
                delivery.AssignedDriverId = driverId;
                delivery.Status = ScheduleStatus.Confirmed;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Delivery assigned to {driver.FullName}";
            }

            return RedirectToAction("AssignDrivers");
        }

        [HttpGet("Schedule")]
        public async Task<IActionResult> Schedule()
        {
            var deliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Include(d => d.AssignedDriver)
                .Select(d => new
                {
                    d.ScheduleNumber,
                    ClientFullName = d.Booking.Client.FullName,
                    UnitNumber = d.Booking.StorageUnit.UnitNumber,
                    d.ScheduledDate,
                    DeliveryType = d.DeliveryType.ToString(),
                    AssignedDriverFullName = d.AssignedDriver != null ? d.AssignedDriver.FullName : "Unassigned",
                    Status = d.Status.ToString()
                })
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            ViewBag.Deliveries = deliveries;
            return View();
        }

        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            var totalDeliveries = await _context.DeliverySchedules.CountAsync();
            var pendingCount = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending);
            var confirmedCount = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Confirmed);
            var completedCount = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Completed);
            var cancelledCount = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Cancelled);

            // FIX: Group by Status properly
            var monthlyDeliveries = await _context.DeliverySchedules
                .Where(d => d.ScheduledDate.Year == DateTime.Today.Year && d.ScheduledDate.Month == DateTime.Today.Month)
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TotalDeliveries = totalDeliveries;
            ViewBag.PendingCount = pendingCount;
            ViewBag.CompletedCount = completedCount;
            ViewBag.CancelledCount = cancelledCount;
            ViewBag.MonthlyDeliveries = monthlyDeliveries;

            return View();
        }

        [HttpGet("Calendar")]
        public IActionResult Calendar()
        {
            return View();
        }

        [HttpGet("GetCalendarEvents")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Select(d => new
                {
                    id = d.Id,
                    title = $"{d.DeliveryType} - {(d.Booking != null && d.Booking.Client != null ? d.Booking.Client.FullName : "Unknown")}",
                    start = d.ScheduledDate.ToString("yyyy-MM-dd"),
                    status = d.Status.ToString(),
                    allDay = true,
                    color = d.Status == ScheduleStatus.Completed ? "#198754" :
                            d.Status == ScheduleStatus.Cancelled ? "#dc3545" :
                            d.Status == ScheduleStatus.Confirmed ? "#0dcaf0" : "#ffc107"
                })
                .ToListAsync();

            return Json(schedules);
        }
    }
}