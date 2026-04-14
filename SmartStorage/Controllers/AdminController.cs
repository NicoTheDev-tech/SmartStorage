using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Core.Entities;
using SmartStorage.Core.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Admin")]
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        [HttpGet("Admin/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Count ONLY ACTIVE/CONFIRMED bookings (NOT cancelled)
            var totalBookings = await _context.Bookings.CountAsync(b => b.Status != BookingStatus.Cancelled);
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            var pendingContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.PendingAcceptance);
            var totalStorageUnits = await _context.StorageUnits.CountAsync();
            var availableUnits = await _context.StorageUnits.CountAsync(u => u.IsActive);
            var pendingPayments = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);

            // Count ONLY payments from confirmed bookings (NOT cancelled)
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.Booking.Status != BookingStatus.Cancelled)
                .SumAsync(p => p.Amount);

            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == DateTime.Now.Month && p.Booking.Status != BookingStatus.Cancelled)
                .SumAsync(p => p.Amount);

            var activeUsers = await _context.Users.CountAsync();
            var pendingDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending);

            // ONLY show active bookings (NOT cancelled)
            var recentBookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .Where(b => b.Status != BookingStatus.Cancelled)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBookingDto
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber ?? string.Empty,
                    ClientName = b.Client != null ? (b.Client.FullName ?? "Unknown") : "Unknown",
                    UnitNumber = b.StorageUnit != null ? (b.StorageUnit.UnitNumber ?? "Unknown") : "Unknown",
                    Status = b.Status.ToString(),
                    Amount = b.TotalAmount,
                    Date = b.CreatedAt
                })
                .ToListAsync();

            // ONLY show payments from confirmed bookings
            var recentPayments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Where(p => p.Status == PaymentStatus.Completed && p.Booking != null && p.Booking.Status != BookingStatus.Cancelled)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new RecentPaymentDto
                {
                    Id = p.Id,
                    PaymentReference = p.PaymentReference ?? string.Empty,
                    ClientName = p.Booking != null && p.Booking.Client != null
                        ? (p.Booking.Client.FullName ?? "Unknown")
                        : "Unknown",
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    Date = p.PaymentDate
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                ActiveContracts = activeContracts,
                PendingContracts = pendingContracts,
                TotalStorageUnits = totalStorageUnits,
                AvailableUnits = availableUnits,
                PendingPayments = pendingPayments,
                TotalRevenue = totalRevenue,
                MonthlyRevenue = monthlyRevenue,
                ActiveUsers = activeUsers,
                PendingDeliveries = pendingDeliveries,
                RecentBookings = recentBookings,
                RecentPayments = recentPayments
            };

            return View(viewModel);
        }

        [HttpGet("Admin/Reservations")]
        public async Task<IActionResult> Reservations()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bookings);
        }

        [HttpPost("Admin/Reservations/UpdateStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                // Prevent approving a cancelled booking
                if (status == "Confirmed" && booking.Status == BookingStatus.Cancelled)
                {
                    TempData["Error"] = "Cannot approve a cancelled booking";
                    return RedirectToAction("Reservations");
                }

                // Prevent cancelling an already cancelled booking
                if (status == "Cancelled" && booking.Status == BookingStatus.Cancelled)
                {
                    TempData["Error"] = "Booking is already cancelled";
                    return RedirectToAction("Reservations");
                }

                booking.Status = Enum.Parse<BookingStatus>(status);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking status updated to {status} successfully";
            }
            return RedirectToAction("Reservations");
        }

        [HttpGet("Admin/Contracts")]
        public async Task<IActionResult> Contracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Client)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(contracts);
        }

        [HttpGet("Admin/Billing")]
        public async Task<IActionResult> Billing()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Client)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }

        [HttpGet("Admin/StorageUnits")]
        public async Task<IActionResult> StorageUnits()
        {
            var units = await _context.StorageUnits.ToListAsync();
            return View(units);
        }

        [HttpGet("Admin/Delivery")]
        public async Task<IActionResult> Delivery()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .OrderByDescending(d => d.ScheduledDate)
                .Select(d => new DeliveryScheduleResponseDto
                {
                    Id = d.Id,
                    ScheduleNumber = d.ScheduleNumber ?? string.Empty,
                    ClientName = d.Booking != null && d.Booking.Client != null ? d.Booking.Client.FullName : "Unknown",
                    UnitNumber = d.Booking != null && d.Booking.StorageUnit != null ? d.Booking.StorageUnit.UnitNumber : "Unknown",
                    DeliveryType = d.DeliveryType.ToString(),
                    ScheduledDate = d.ScheduledDate,
                    TimeSlot = d.TimeSlot ?? string.Empty,
                    ItemCount = d.ItemCount,
                    Status = d.Status.ToString()
                })
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet("Admin/Delivery/Details/{id}")]
        public async Task<IActionResult> DeliveryDetails(int id)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Include(d => d.AssignedDriver)
                .Where(d => d.Id == id)
                .Select(d => new DeliveryScheduleResponseDto
                {
                    Id = d.Id,
                    ScheduleNumber = d.ScheduleNumber ?? string.Empty,
                    ClientName = d.Booking != null && d.Booking.Client != null ? d.Booking.Client.FullName : "Unknown",
                    UnitNumber = d.Booking != null && d.Booking.StorageUnit != null ? d.Booking.StorageUnit.UnitNumber : "Unknown",
                    DeliveryType = d.DeliveryType.ToString(),
                    ScheduledDate = d.ScheduledDate,
                    TimeSlot = d.TimeSlot ?? string.Empty,
                    ItemCount = d.ItemCount,
                    Status = d.Status.ToString(),
                    PickupAddress = d.PickupAddress ?? string.Empty,
                    DeliveryAddress = d.DeliveryAddress ?? string.Empty,
                    GoodsDescription = d.GoodsDescription ?? string.Empty,
                    EstimatedWeight = d.EstimatedWeight,
                    SpecialInstructions = d.SpecialInstructions ?? string.Empty,
                    ContactPerson = d.ContactPerson ?? string.Empty,
                    ContactPhone = d.ContactPhone ?? string.Empty,
                    AssignedDriver = d.AssignedDriver != null ? d.AssignedDriver.FullName : "Not Assigned"
                })
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpGet("Admin/Reports")]
        public async Task<IActionResult> Reports()
        {
            var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);
            var monthlyRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == DateTime.Now.Month)
                .SumAsync(p => p.Amount);
            var totalBookings = await _context.Bookings.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);
            var totalUsers = await _context.Users.CountAsync();
            var availableUnits = await _context.StorageUnits.CountAsync(u => u.IsActive);
            var pendingPayments = await _context.Payments.Where(p => p.Status == PaymentStatus.Pending).SumAsync(p => p.Amount);
            var completedPayments = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);
            var pendingDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending);
            var completedDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Completed);
            var confirmedDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Confirmed);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.AvailableUnits = availableUnits;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.CompletedPayments = completedPayments;
            ViewBag.PendingDeliveries = pendingDeliveries;
            ViewBag.CompletedDeliveries = completedDeliveries;
            ViewBag.ConfirmedDeliveries = confirmedDeliveries;

            return View();
        }

        [HttpGet("Admin/Delivery/Calendar")]
        public IActionResult DeliveryCalendar()
        {
            return View();
        }

        [HttpGet("Admin/Delivery/GetCalendarEvents")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Select(d => new
                {
                    id = d.Id,
                    title = $"{d.DeliveryType} - {d.Booking.Client.FullName}",
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

        [HttpGet("Admin/Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet("Admin/StorageUnits/Edit/{id}")]
        public async Task<IActionResult> EditUnit(int id)
        {
            var unit = await _context.StorageUnits.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            return View(unit);
        }

        [HttpPost("Admin/StorageUnits/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(int id, StorageUnit unit)
        {
            if (id != unit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Storage unit updated successfully";
                return RedirectToAction("StorageUnits");
            }
            return View(unit);
        }

        // ============ CONTRACT ACTIONS ============

        [HttpGet("Admin/Contracts/Details/{id}")]
        public async Task<IActionResult> ContractDetails(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Client)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpPost("Admin/ApproveContract/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null && contract.Status == ContractStatus.PendingAcceptance)
            {
                contract.Status = ContractStatus.Accepted;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract approved successfully";
            }
            return RedirectToAction("Contracts");
        }

        [HttpPost("Admin/ActivateContract/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null && contract.Status == ContractStatus.Accepted)
            {
                contract.Status = ContractStatus.Active;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contract activated successfully";
            }
            return RedirectToAction("Contracts");
        }

        // Remove this button by NOT adding a Create action - or add this to hide it:
        [HttpGet("Admin/Contracts/Create")]
        public IActionResult CreateContract()
        {
            // Redirect to Reservations instead since contracts are created from bookings
            return RedirectToAction("Reservations");
        }

        // ============ STORAGE UNIT ACTIONS ============

        [HttpGet("Admin/StorageUnits/Create")]
        public IActionResult CreateUnit()
        {
            return View(new StorageUnit());
        }

        [HttpPost("Admin/StorageUnits/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUnit(StorageUnit unit)
        {
            if (ModelState.IsValid)
            {
                _context.StorageUnits.Add(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Storage unit created successfully";
                return RedirectToAction("StorageUnits");
            }
            return View(unit);
        }

        // ============ USER MANAGEMENT ACTIONS ============

        [HttpPost("Admin/UnblockUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid user ID";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "User unblocked successfully";
            }
            else
            {
                TempData["Error"] = "User not found";
            }

            return RedirectToAction("Users");
        }

        [HttpPost("Admin/BlockUser/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid user ID";
                return RedirectToAction("Users");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _userManager.UpdateAsync(user);
                TempData["Success"] = "User blocked successfully";
            }
            else
            {
                TempData["Error"] = "User not found";
            }

            return RedirectToAction("Users");
        }
    }
}