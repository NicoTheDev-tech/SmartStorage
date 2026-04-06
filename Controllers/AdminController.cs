using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Infrastructure.Data;
using SmartStorage.ViewModels;
using SmartStorage.Core.Entities;
using System.Linq;
using System.Threading.Tasks;
using SmartStorage.Core.DTOs;

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalBookings = await _context.Bookings.CountAsync(),
                PendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending),
                ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active),
                PendingContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.PendingAcceptance),
                TotalStorageUnits = await _context.StorageUnits.CountAsync(),
                AvailableUnits = await _context.StorageUnits.CountAsync(u => u.IsActive),
                PendingPayments = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending),
                TotalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount),
                MonthlyRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Month == DateTime.Now.Month).SumAsync(p => p.Amount),
                ActiveUsers = await _context.Users.CountAsync(),
                PendingDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending),
                RecentBookings = await _context.Bookings
                    .Include(b => b.Client)
                    .Include(b => b.StorageUnit)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .Select(b => new RecentBookingDto
                    {
                        Id = b.Id,
                        BookingNumber = b.BookingNumber ?? string.Empty,
                        ClientName = b.Client != null ? b.Client.FullName ?? "Unknown" : "Unknown",
                        UnitNumber = b.StorageUnit != null ? b.StorageUnit.UnitNumber ?? "Unknown" : "Unknown",
                        Status = b.Status.ToString(),
                        Amount = b.TotalAmount,
                        Date = b.CreatedAt
                    }).ToListAsync(),
                RecentPayments = await _context.Payments
                    .Include(p => p.Booking)
                        .ThenInclude(b => b != null ? b.Client : null)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .Select(p => new RecentPaymentDto
                    {
                        Id = p.Id,
                        PaymentReference = p.PaymentReference ?? string.Empty,
                        ClientName = p.Booking != null && p.Booking.Client != null ? p.Booking.Client.FullName ?? "Unknown" : "Unknown",
                        Amount = p.Amount,
                        Status = p.Status.ToString(),
                        Date = p.PaymentDate
                    }).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet("Reservations")]
        public async Task<IActionResult> Reservations()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bookings);
        }

        [HttpPost("Reservations/UpdateStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = Enum.Parse<BookingStatus>(status);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking status updated successfully";
            }
            return RedirectToAction("Reservations");
        }

        [HttpGet("Contracts")]
        public async Task<IActionResult> Contracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(contracts);
        }

        [HttpPost("Contracts/Approve/{id}")]
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

        [HttpPost("Contracts/Activate/{id}")]
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

        [HttpGet("Billing")]
        public async Task<IActionResult> Billing()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }

        [HttpPost("Billing/Verify/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null && payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Completed;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment verified successfully";
            }
            return RedirectToAction("Billing");
        }

        [HttpGet("StorageUnits")]
        public async Task<IActionResult> StorageUnits()
        {
            var units = await _context.StorageUnits.ToListAsync();
            return View(units);
        }

        [HttpGet("StorageUnits/Create")]
        public IActionResult CreateUnit()
        {
            return View();
        }

        [HttpPost("StorageUnits/Create")]
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

        [HttpGet("StorageUnits/Edit/{id}")]
        public async Task<IActionResult> EditUnit(int id)
        {
            var unit = await _context.StorageUnits.FindAsync(id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost("StorageUnits/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(int id, StorageUnit unit)
        {
            if (id != unit.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Storage unit updated successfully";
                return RedirectToAction("StorageUnits");
            }
            return View(unit);
        }

        [HttpPost("StorageUnits/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _context.StorageUnits.FindAsync(id);
            if (unit != null)
            {
                _context.StorageUnits.Remove(unit);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Storage unit deleted successfully";
            }
            return RedirectToAction("StorageUnits");
        }

        [HttpGet("Delivery")]
        public async Task<IActionResult> Delivery()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();
            return View(schedules);
        }

        [HttpGet("Delivery/Details/{id}")]
        public async Task<IActionResult> DeliveryDetails(int id)
        {
            if (id <= 0)
                return NotFound();

            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .Include(d => d.AssignedDriver)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost("Delivery/Confirm/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.Pending)
            {
                schedule.Status = ScheduleStatus.Confirmed;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery schedule confirmed";
            }
            return RedirectToAction("Delivery");
        }

        [HttpGet("Delivery/Calendar")]
        public IActionResult DeliveryCalendar()
        {
            return View();
        }

        [HttpGet("Delivery/GetCalendarEvents")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .ToListAsync();

            var events = schedules.Select(s => new
            {
                id = s.Id,
                title = $"{s.DeliveryType} - {(s.Booking?.Client?.FullName ?? "Unknown Client")}",
                start = s.ScheduledDate.ToString("yyyy-MM-dd"),
                extendedProps = new { status = s.Status.ToString() },
                allDay = true,
                color = s.Status.ToString() switch
                {
                    "Pending" => "#ffc107",
                    "Confirmed" => "#0dcaf0",
                    "Completed" => "#198754",
                    "Cancelled" => "#dc3545",
                    _ => "#6c757d"
                }
            });

            return Json(events);
        }


        [HttpPost("Delivery/Complete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteDelivery(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null && schedule.Status == ScheduleStatus.Confirmed)
            {
                schedule.Status = ScheduleStatus.Completed;
                schedule.CompletedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Delivery marked as completed";
            }
            return RedirectToAction("Delivery");
        }

        [HttpGet("Reports")]
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
            var completedPayments = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);
            var pendingPayments = await _context.Payments.Where(p => p.Status == PaymentStatus.Pending).SumAsync(p => p.Amount);
            var pendingDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Pending);
            var completedDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Completed);
            var confirmedDeliveries = await _context.DeliverySchedules.CountAsync(d => d.Status == ScheduleStatus.Confirmed);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.ActiveContracts = activeContracts;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.AvailableUnits = availableUnits;
            ViewBag.CompletedPayments = completedPayments;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.PendingDeliveries = pendingDeliveries;
            ViewBag.CompletedDeliveries = completedDeliveries;
            ViewBag.ConfirmedDeliveries = confirmedDeliveries;

            return View();
        }

        [HttpGet("Users")]
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

        [HttpPost("Users/Block/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User blocked successfully";
            }
            return RedirectToAction("Users");
        }

        [HttpPost("Users/Unblock/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User unblocked successfully";
            }
            return RedirectToAction("Users");
        }
    }
}