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
    [Authorize(Roles = "Driver")]
    [Route("Driver")]
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DriverController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var today = DateTime.Today;

            // Get today's assigned deliveries
            var todaysDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.AssignedDriverId == driver.Id && d.ScheduledDate.Date == today && d.Status != ScheduleStatus.Completed)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            var upcomingDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                .Where(d => d.AssignedDriverId == driver.Id && d.ScheduledDate.Date > today && d.Status != ScheduleStatus.Completed)
                .OrderBy(d => d.ScheduledDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Driver = driver;
            ViewBag.TodaysDeliveries = todaysDeliveries;
            ViewBag.UpcomingDeliveries = upcomingDeliveries;

            return View();
        }

        [HttpGet("CompleteProfile")]
        public IActionResult CompleteProfile()
        {
            return View();
        }

        [HttpPost("CompleteProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(string fullName, string licenseNumber, string phone)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var driver = new Driver
            {
                UserId = userId,
                FullName = fullName,
                EmployeeNumber = $"DRV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Phone = phone,
                Email = User.Identity?.Name ?? "",
                LicenseNumber = licenseNumber,
                IsAvailable = true,
                DriverStatus = DriverStatus.Available,
                Role = StaffRole.Driver,
                Status = StaffStatus.Active,
                HireDate = DateTime.Now
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpPost("CompleteDelivery/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteDelivery(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery marked as completed";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpGet("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            ViewBag.CurrentStatus = driver.DriverStatus;
            return View();
        }

        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver != null)
            {
                driver.DriverStatus = status switch
                {
                    "Available" => DriverStatus.Available,
                    "OnDelivery" => DriverStatus.OnDelivery,
                    "Break" => DriverStatus.Break,
                    "OffDuty" => DriverStatus.OffDuty,
                    _ => driver.DriverStatus
                };
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Status updated to {status}";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet("MyDeliveries")]
        public async Task<IActionResult> MyDeliveries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var deliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.AssignedDriverId == driver.Id && d.Status != ScheduleStatus.Completed && d.Status != ScheduleStatus.Cancelled)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            return View(deliveries);
        }

        [HttpGet("ExecuteDelivery/{id}")]
        public async Task<IActionResult> ExecuteDelivery(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .FirstOrDefaultAsync(d => d.Id == id && d.AssignedDriverId == driver.Id);

            if (schedule == null)
            {
                TempData["Error"] = "Delivery not found.";
                return RedirectToAction("MyDeliveries");
            }

            return View(schedule);
        }

        [HttpPost("ExecuteDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteDeliveryConfirmed(int id, string proofOfDelivery, string customerSignature, string notes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(d => d.Id == id && d.AssignedDriverId == driver.Id);

            if (schedule == null)
            {
                TempData["Error"] = "Delivery not found.";
                return RedirectToAction("MyDeliveries");
            }

            schedule.Status = ScheduleStatus.Completed;
            schedule.CompletedAt = DateTime.Now;
            schedule.ProofOfDelivery = proofOfDelivery;
            schedule.CustomerSignature = customerSignature;
            schedule.DriverNotes = notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Delivery completed successfully!";
            return RedirectToAction("MyDeliveries");
        }
    }
    }