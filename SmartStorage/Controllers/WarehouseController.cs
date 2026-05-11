using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;  // Add this using

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "WarehouseStaff")]
    [Route("Warehouse")]
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WarehouseController> _logger;  // Add this

        public WarehouseController(ApplicationDbContext context, ILogger<WarehouseController> logger)  // Add logger parameter
        {
            _context = context;
            _logger = logger;  // Add this
        }

        // Rest of your existing methods...
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var staff = await _context.WarehouseStaff.FirstOrDefaultAsync(w => w.UserId == userId);

            if (staff == null)
            {
                return RedirectToAction("CompleteProfile");
            }

            var pendingIntake = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Where(d => d.DeliveryType == DeliveryType.Dropoff && d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            var todayPickups = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Where(d => d.DeliveryType == DeliveryType.Collection && d.ScheduledDate.Date == DateTime.Today && d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            ViewBag.Staff = staff;
            ViewBag.PendingIntake = pendingIntake;
            ViewBag.TodayPickups = todayPickups;

            return View();
        }

        [HttpGet("CompleteProfile")]
        public IActionResult CompleteProfile()
        {
            return View();
        }

        [HttpPost("CompleteProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(string fullName, string phone, string assignedZone)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var staff = new WarehouseStaff
            {
                UserId = userId,
                FullName = fullName,
                EmployeeNumber = $"WH-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Phone = phone,
                Email = User.Identity?.Name ?? "",
                AssignedZone = assignedZone,
                WarehouseRole = WarehouseRole.Receiver,
                Role = StaffRole.WarehouseStaff,
                Status = StaffStatus.Active,
                HireDate = DateTime.Now
            };

            _context.WarehouseStaff.Add(staff);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpPost("PrepareDelivery/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrepareDelivery(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (schedule != null && schedule.Status == ScheduleStatus.Pending)
            {
                schedule.Status = ScheduleStatus.Confirmed;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery prepared and ready for driver assignment";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpGet("GoodsIntake")]
        public async Task<IActionResult> GoodsIntake()
        {
            var pendingIntake = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Status == ScheduleStatus.Pending && d.DeliveryType == DeliveryType.Dropoff)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            return View(pendingIntake);
        }

        [HttpPost("ReceiveGoods/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveGoods(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.Pending)
            {
                schedule.Status = ScheduleStatus.InProgress;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Goods received, now storing...";
            }
            return RedirectToAction("GoodsIntake");
        }

        [HttpPost("StoreGoods/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreGoods(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.InProgress)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Goods stored successfully";
            }
            return RedirectToAction("GoodsIntake");
        }

        [HttpGet("StorageMap")]
        public async Task<IActionResult> StorageMap()
        {
            var units = await _context.StorageUnits
                .OrderBy(u => u.Location)
                .ThenBy(u => u.UnitNumber)
                .ToListAsync();

            return View(units);
        }

        [HttpGet("Inventory")]
        public async Task<IActionResult> Inventory(string searchTerm, string status, string climateControl, int page = 1)
        {
            try
            {
                // Start with base query
                var query = _context.StorageUnits.AsQueryable();

                // Apply search filter (Unit Number or Location)
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.UnitNumber.Contains(searchTerm) ||
                                              (u.Location != null && u.Location.Contains(searchTerm)));
                }

                // Apply status filter (Active/Inactive)
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    bool isActive = status == "true";
                    query = query.Where(u => u.IsActive == isActive);
                }

                // Apply climate control filter
                if (!string.IsNullOrEmpty(climateControl) && climateControl != "All")
                {
                    query = query.Where(u => u.ClimateControl == climateControl);
                }

                // Order by Unit Number
                var units = await query
                    .OrderBy(u => u.UnitNumber)
                    .ToListAsync();

                // Store filter values in ViewBag to persist in the form
                ViewBag.SearchTerm = searchTerm ?? "";
                ViewBag.Status = status ?? "All";
                ViewBag.ClimateControl = climateControl ?? "All";

                return View(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory");
                TempData["Error"] = "Error loading inventory data";
                return View(new List<StorageUnit>());
            }
        }

        [HttpGet("AddUnit")]
        public IActionResult AddUnit()
        {
            return View(new StorageUnit());
        }

        [HttpPost("AddUnit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(StorageUnit unit)
        {
            if (ModelState.IsValid)
            {
                // Check if unit number already exists
                var existingUnit = await _context.StorageUnits
                    .FirstOrDefaultAsync(u => u.UnitNumber == unit.UnitNumber);

                if (existingUnit != null)
                {
                    ModelState.AddModelError("UnitNumber", "Unit number already exists");
                    return View(unit);
                }

                // Set default values if not provided
                if (string.IsNullOrEmpty(unit.ClimateControl))
                    unit.ClimateControl = "None";

                unit.IsActive = true;

                _context.StorageUnits.Add(unit);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Unit {unit.UnitNumber} has been added successfully!";
                return RedirectToAction("Inventory");
            }

            return View(unit);
        }

        [HttpGet("EditUnit/{id}")]
        public async Task<IActionResult> EditUnit(int id)
        {
            var unit = await _context.StorageUnits.FindAsync(id);
            if (unit == null)
            {
                TempData["Error"] = "Unit not found";
                return RedirectToAction("Inventory");
            }

            return View(unit);
        }

        [HttpPost("EditUnit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(int id, StorageUnit unit)
        {
            if (id != unit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if unit number already exists (excluding current unit)
                    var existingUnit = await _context.StorageUnits
                        .FirstOrDefaultAsync(u => u.UnitNumber == unit.UnitNumber && u.Id != id);

                    if (existingUnit != null)
                    {
                        ModelState.AddModelError("UnitNumber", "Unit number already exists");
                        return View(unit);
                    }

                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Unit {unit.UnitNumber} has been updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.StorageUnits.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction("Inventory");
            }

            return View(unit);
        }

        [HttpPost("ToggleUnitStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUnitStatus(int id, bool isActive)
        {
            var unit = await _context.StorageUnits.FindAsync(id);
            if (unit != null)
            {
                unit.IsActive = isActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Unit {unit.UnitNumber} has been {(isActive ? "activated" : "deactivated")}";
            }
            return RedirectToAction("Inventory");
        }

        [HttpGet("GetUnitDetails/{id}")]
        public async Task<IActionResult> GetUnitDetails(int id)
        {
            var unit = await _context.StorageUnits.FindAsync(id);

            if (unit == null)
            {
                return Content("<div class='alert alert-danger'>Unit not found</div>");
            }

            var html = $@"
        <dl class='row'>
            <dt class='col-sm-4'>Unit Number:</dt>
            <dd class='col-sm-8'>{unit.UnitNumber}</dd>

            <dt class='col-sm-4'>Size:</dt>
            <dd class='col-sm-8'>{unit.Size}</dd>

            <dt class='col-sm-4'>Location:</dt>
            <dd class='col-sm-8'>{unit.Location}</dd>

            <dt class='col-sm-4'>Climate Control:</dt>
            <dd class='col-sm-8'>{unit.ClimateControl}</dd>

            <dt class='col-sm-4'>Monthly Rate:</dt>
            <dd class='col-sm-8'>R {unit.MonthlyRate:N2}</dd>

            <dt class='col-sm-4'>Status:</dt>
            <dd class='col-sm-8'>{(unit.IsActive ? "<span class='badge bg-success'>Active</span>" : "<span class='badge bg-danger'>Inactive</span>")}</dd>
        </dl>
    ";

            return Content(html);
        }

        [HttpGet("Occupancy")]
        public async Task<IActionResult> Occupancy()
        {
            var totalUnits = await _context.StorageUnits.CountAsync();

            // Get occupied units (Confirmed or Active bookings) - using enum values
            var occupiedUnits = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active)
                .Select(b => b.StorageUnitId)
                .Distinct()
                .CountAsync();

            // Get reserved units (Pending bookings)
            var reservedUnits = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.StorageUnitId)
                .Distinct()
                .CountAsync();

            var availableUnits = totalUnits - (occupiedUnits + reservedUnits);

            var occupancyData = new
            {
                Total = totalUnits,
                Occupied = occupiedUnits,
                Reserved = reservedUnits,
                Available = availableUnits,
                OccupancyPercentage = totalUnits > 0 ? (occupiedUnits * 100) / totalUnits : 0
            };

            ViewBag.Occupancy = occupancyData;

            // Get occupied units with customer details - using enum values
            var occupiedUnitsList = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Include(b => b.Client)
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active)
                .Select(b => new {
                    b.StorageUnit,
                    b.Client,
                    b.CreatedAt,
                    b.EndDate
                })
                .ToListAsync();

            ViewBag.OccupiedUnits = occupiedUnitsList;

            return View();
        }

        [HttpGet("GetAvailableDrivers")]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            var drivers = await _context.Drivers
                .Where(d => d.Status == StaffStatus.Active)
                .Select(d => new { d.Id, d.FullName, d.VehicleAssigned, d.Phone })
                .ToListAsync();

            return Json(drivers);
        }

        [HttpPost("AssignDriver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int scheduleId, int driverId)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(scheduleId);
            var driver = await _context.Drivers.FindAsync(driverId);

            if (schedule == null || driver == null)
            {
                return Json(new { success = false, message = "Schedule or driver not found" });
            }

            schedule.AssignedDriverId = driverId;
            schedule.Status = ScheduleStatus.Confirmed;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {driver.FullName} assigned to delivery {schedule.ScheduleNumber}";
            return Json(new { success = true, message = "Driver assigned successfully" });
        }

        [HttpGet("Warehouse/PrepareTransport/{scheduleId}")]
        public async Task<IActionResult> PrepareTransport(int scheduleId)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Include(d => d.AssignedDriver)
                .FirstOrDefaultAsync(d => d.Id == scheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpPost("Warehouse/PrepareTransport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrepareTransport(int scheduleId, bool goodsVerified, bool vehicleChecked, bool paperworkReady, string? notes)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(scheduleId);

            if (schedule == null)
            {
                TempData["Error"] = "Transport job not found.";
                return RedirectToAction("Dashboard");
            }

            if (!goodsVerified || !vehicleChecked || !paperworkReady)
            {
                TempData["Error"] = "All preparation steps must be completed before starting the job.";
                return RedirectToAction("PrepareTransport", new { scheduleId = scheduleId });
            }

            schedule.Status = ScheduleStatus.InProgress;
            schedule.PreparedAt = DateTime.Now;
            schedule.PreparedBy = User.Identity?.Name;
            schedule.PreparationNotes = notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Transport job {schedule.ScheduleNumber} has been prepared and is ready for execution.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Warehouse/ReadyJobs")]
        public async Task<IActionResult> ReadyJobs()
        {
            var jobs = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.AssignedDriver)
                .Where(d => d.Status == ScheduleStatus.Confirmed && d.PreparedAt == null)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            return View(jobs);
        }
    }
}