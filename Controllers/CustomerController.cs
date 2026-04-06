using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using SmartStorage.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    [Route("Customer")]
    public class CustomerController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IContractService _contractService;
        private readonly IInvoiceService _invoiceService;
        private readonly IDeliveryScheduleService _deliveryService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public CustomerController(
            IBookingService bookingService,
            IContractService contractService,
            IInvoiceService invoiceService,
            IDeliveryScheduleService deliveryService,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _bookingService = bookingService;
            _contractService = contractService;
            _invoiceService = invoiceService;
            _deliveryService = deliveryService;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            var bookings = await _bookingService.GetClientBookingsByUserId(userId);
            var contracts = await _contractService.GetClientContracts(userId);
            var invoices = await _invoiceService.GetClientInvoices(userId);
            var schedules = await _deliveryService.GetClientSchedules(userId);

            var viewModel = new CustomerDashboardViewModel
            {
                ActiveBookingsCount = bookings.Count(b => b.Status == "Confirmed" || b.Status == "Active"),
                ActiveContractsCount = contracts.Count(c => c.Status == "Active"),
                PendingInvoicesCount = invoices.Count(i => i.Status == "Pending" || i.Status == "Overdue"),
                UpcomingDeliveriesCount = schedules.Count(s => s.Status == "Pending" || s.Status == "Confirmed"),
                TotalOutstandingBalance = invoices.Where(i => i.Status != "Paid").Sum(i => i.Balance),
                RecentBookings = bookings.Take(5),
                RecentContracts = contracts.Take(5),
                RecentInvoices = invoices.Take(5),
                ClientName = client?.FullName,
                ClientPreferredName = client?.PreferredName,
                ClientEmail = client?.Email,
                ClientPhone = client?.Phone
            };

            return View(viewModel);
        }

        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (client == null)
            {
                client = new Core.Entities.Client { UserId = userId };
            }

            var viewModel = new UpdateProfileViewModel
            {
                FullName = client.FullName ?? string.Empty,
                PreferredName = client.PreferredName ?? string.Empty,
                Email = client.Email ?? string.Empty,
                Phone = client.Phone ?? string.Empty,
                IdNumber = client.IdNumber ?? string.Empty,
                Address = client.Address ?? string.Empty
            };

            return View(viewModel);
        }

        [HttpPost("Profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
            {
                client = new Core.Entities.Client { UserId = userId };
                _context.Clients.Add(client);
            }

            client.FullName = model.FullName ?? string.Empty;
            client.PreferredName = model.PreferredName ?? string.Empty;
            client.Email = model.Email ?? string.Empty;
            client.Phone = model.Phone ?? string.Empty;
            client.IdNumber = model.IdNumber ?? string.Empty;
            client.Address = model.Address ?? string.Empty;

            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var existingClaims = await _userManager.GetClaimsAsync(user);
                var preferredNameClaim = existingClaims.FirstOrDefault(c => c.Type == "PreferredName");
                if (preferredNameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, preferredNameClaim);
                }
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("PreferredName", client.PreferredName));
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        [HttpGet("ChangePassword")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Your password has been changed successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet("MyStorage")]
        public async Task<IActionResult> MyStorage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var contracts = await _contractService.GetClientContracts(userId ?? string.Empty);
            var activeContracts = contracts.Where(c => c.Status == "Active");
            return View(activeContracts);
        }

        [HttpGet("MyBookings")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingService.GetClientBookingsByUserId(userId ?? string.Empty);
            return View(bookings);
        }

        [HttpGet("MyContracts")]
        public async Task<IActionResult> MyContracts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var contracts = await _contractService.GetClientContracts(userId ?? string.Empty);
            return View(contracts);
        }

        [HttpGet("Invoices")]
        public async Task<IActionResult> Invoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invoices = await _invoiceService.GetClientInvoices(userId ?? string.Empty);

            // Debug - write to file
            System.IO.File.WriteAllText(@"C:\Users\infoi\Desktop\invoice_debug.txt",
                $"Number of invoices: {invoices.Count()}\n");

            foreach (var inv in invoices)
            {
                System.IO.File.AppendAllText(@"C:\Users\infoi\Desktop\invoice_debug.txt",
                    $"Invoice: {inv.InvoiceNumber}, Amount: {inv.Amount}, Status: {inv.Status}\n");
            }

            return View(invoices);
        }

        [HttpGet("Payments")]
        public async Task<IActionResult> Payments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invoices = await _invoiceService.GetClientInvoices(userId ?? string.Empty);
            var payments = invoices.Where(i => i.Status == "Paid" || i.AmountPaid > 0);
            return View(payments);
        }

        [HttpGet("PaymentHistory")]
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invoices = await _invoiceService.GetClientInvoices(userId ?? string.Empty);
            var paymentHistory = invoices.Where(i => i.Status == "Paid" || i.PaymentDate.HasValue);
            return View(paymentHistory);
        }

        [HttpGet("Cartage")]
        public async Task<IActionResult> Cartage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var schedules = await _deliveryService.GetClientSchedules(userId ?? string.Empty);
            return View(schedules);
        }

        [HttpGet("VehicleServices")]
        public IActionResult VehicleServices()
        {
            return View();
        }

        [HttpGet("DeliverySchedule")]
        public async Task<IActionResult> DeliverySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var schedules = await _deliveryService.GetClientSchedules(userId ?? string.Empty);
            return View(schedules);
        }

        [HttpGet("CreateDeliverySchedule")]
        public async Task<IActionResult> CreateDeliverySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _bookingService.GetClientBookingsByUserId(userId ?? string.Empty);
            var activeBookings = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Active");

            ViewBag.Bookings = activeBookings;
            ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(DateTime.Today);

            return View(new CreateDeliveryScheduleDto());
        }

        [HttpPost("CreateDeliverySchedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeliverySchedule(CreateDeliveryScheduleDto model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetClientBookingsByUserId(userId ?? string.Empty);
                ViewBag.Bookings = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Active");
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(model.ScheduledDate);
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.CreateSchedule(model, userId ?? string.Empty);
                TempData["Success"] = $"Delivery schedule {schedule.ScheduleNumber} created successfully!";
                return RedirectToAction("DeliverySchedule");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetClientBookingsByUserId(userId ?? string.Empty);
                ViewBag.Bookings = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Active");
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(model.ScheduledDate);
                return View(model);
            }
        }

        [HttpGet("DeliveryScheduleDetails/{id}")]
        public async Task<IActionResult> DeliveryScheduleDetails(int id)
        {
            var schedule = await _deliveryService.GetScheduleById(id);
            if (schedule == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientSchedules = await _deliveryService.GetClientSchedules(userId ?? string.Empty);
            if (!clientSchedules.Any(s => s.Id == id))
                return Forbid();

            return View(schedule);
        }

        [HttpGet("RescheduleDelivery/{id}")]
        public async Task<IActionResult> RescheduleDelivery(int id)
        {
            var schedule = await _deliveryService.GetScheduleById(id);
            if (schedule == null)
                return NotFound();

            if (!schedule.CanReschedule)
            {
                TempData["Error"] = "This schedule cannot be rescheduled";
                return RedirectToAction("DeliverySchedule");
            }

            ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(schedule.ScheduledDate);
            ViewBag.Schedule = schedule;

            return View();
        }

        [HttpPost("RescheduleDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleDelivery(UpdateDeliveryScheduleDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.UpdateSchedule(model, userId ?? string.Empty);
                TempData["Success"] = $"Delivery rescheduled to {schedule.ScheduledDate:dd MMM yyyy} at {schedule.TimeSlot}";
                return RedirectToAction("DeliverySchedule");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var schedule = await _deliveryService.GetScheduleById(model.ScheduleId);
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(model.ScheduledDate ?? schedule?.ScheduledDate ?? DateTime.Today);
                ViewBag.Schedule = schedule;
                return View(model);
            }
        }

        [HttpPost("CancelDeliverySchedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDeliverySchedule(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.CancelSchedule(id, userId ?? string.Empty);
                TempData["Success"] = "Delivery schedule cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("DeliverySchedule");
        }

        [HttpPost("CancelBooking")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            try
            {
                var booking = await _bookingService.CancelBooking(bookingId, userId);
                TempData["Success"] = $"Booking {booking.BookingNumber} has been cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("MyBookings");
        }
    }
}