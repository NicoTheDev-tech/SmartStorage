using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using SmartStorage.Infrastructure.Data;
using SmartStorage.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

#if SupressWarnings
#pragma warning disable CS8602
#pragma warning disable CS0229
#endif

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

            // Get ALL bookings but filter later
            var allBookings = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Where(b => b.Client != null && b.Client.UserId != null && b.Client.UserId == userId)
                .ToListAsync();

            // Get ALL invoices but filter later
            var allInvoices = await _context.Invoices
                .Where(i => i.Client != null && i.Client.UserId != null && i.Client.UserId == userId)
                .ToListAsync();

            // ACTIVE BOOKINGS = Confirmed or Active (NOT Cancelled and NOT Pending)
            var activeBookings = allBookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active).ToList();

            // PENDING INVOICES only
            var pendingInvoices = allInvoices.Where(i => i.Status == InvoiceStatus.Pending).ToList();

            // Recent Bookings - Show only Confirmed/Active bookings (NOT Pending)
            var recentBookings = allBookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Active)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBookingDto
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber ?? string.Empty,
                    UnitNumber = (b.StorageUnit != null && b.StorageUnit.UnitNumber != null) ? b.StorageUnit.UnitNumber : "Unknown",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status.ToString(),
                    Amount = b.TotalAmount,
                    CreatedAt = b.CreatedAt
                })
                .ToList();

            // Recent Invoices - ONLY PENDING
            var recentInvoices = allInvoices
                .Where(i => i.Status == InvoiceStatus.Pending)
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .Select(i => new RecentInvoiceDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber ?? string.Empty,
                    BookingId = i.BookingId,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString(),
                    Balance = i.Balance
                })
                .ToList();

            var viewModel = new CustomerDashboardViewModel
            {
                ActiveBookings = activeBookings.Count,
                ActiveContracts = 0,
                TotalInvoices = pendingInvoices.Count,
                UpcomingDeliveries = 0,
                TotalOutstandingBalance = pendingInvoices.Sum(i => i.Balance),
                RecentBookings = recentBookings,
                RecentInvoices = recentInvoices,
                ClientName = (client != null && client.FullName != null) ? client.FullName : string.Empty,
                ClientPreferredName = (client != null && client.PreferredName != null) ? client.PreferredName : string.Empty,
                ClientEmail = (client != null && client.Email != null) ? client.Email : string.Empty,
                ClientPhone = (client != null && client.Phone != null) ? client.Phone : string.Empty
            };

            return View(viewModel);
        }

        [HttpGet("MyBookings")]
        public async Task<IActionResult> MyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Where(b => b.Client != null && b.Client.UserId != null && b.Client.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    BookingNumber = (b.BookingNumber != null) ? b.BookingNumber : string.Empty,
                    UnitNumber = (b.StorageUnit != null && b.StorageUnit.UnitNumber != null) ? b.StorageUnit.UnitNumber : "Unknown",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
            return View(bookings);
        }

        [HttpGet("MyContracts")]
        public async Task<IActionResult> MyContracts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                .Where(c => c.Client != null && c.Client.UserId != null && c.Client.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ContractResponseDto
                {
                    Id = c.Id,
                    ContractNumber = (c.ContractNumber != null) ? c.ContractNumber : string.Empty,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MonthlyRate = c.MonthlyRate,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
            return View(contracts);
        }

        [HttpGet("Invoices")]
        public async Task<IActionResult> Invoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var invoices = await _context.Invoices
                .Where(i => i.Client != null && i.Client.UserId != null && i.Client.UserId == userId)
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceNumber = (i.InvoiceNumber != null) ? i.InvoiceNumber : string.Empty,
                    BookingId = i.BookingId,
                    Amount = i.Amount,
                    AmountPaid = i.AmountPaid,
                    Balance = i.Balance,
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString(),
                    StatusValue = (int)i.Status
                })
                .ToListAsync();
            return View(invoices);
        }

        [HttpGet("DeliverySchedule")]
        public async Task<IActionResult> DeliverySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Booking != null && d.Booking.Client != null && d.Booking.Client.UserId != null && d.Booking.Client.UserId == userId && d.Status != ScheduleStatus.Cancelled)
                .OrderByDescending(d => d.ScheduledDate)
                .Select(d => new DeliveryScheduleResponseDto
                {
                    Id = d.Id,
                    ScheduleNumber = d.ScheduleNumber ?? string.Empty,
                    ScheduledDate = d.ScheduledDate,
                    TimeSlot = d.TimeSlot ?? string.Empty,
                    Status = d.Status.ToString(),
                    DeliveryType = d.DeliveryType.ToString(),
                    UnitNumber = d.Booking != null && d.Booking.StorageUnit != null
                        ? (d.Booking.StorageUnit.UnitNumber ?? "Unknown")
                        : "Unknown",
                    ItemCount = d.ItemCount
                })
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet("CreateDeliverySchedule")]
        public async Task<IActionResult> CreateDeliverySchedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Get active bookings (confirmed/active, not cancelled)
            var bookings = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Where(b => b.Client != null && b.Client.UserId != null && b.Client.UserId == userId
                    && b.Status != BookingStatus.Cancelled)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber ?? string.Empty,
                    UnitNumber = b.StorageUnit != null ? (b.StorageUnit.UnitNumber ?? "Unknown") : "Unknown",
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    Status = b.Status.ToString(),
                    TotalAmount = b.TotalAmount
                })
                .ToListAsync();

            ViewBag.Bookings = bookings;
            ViewBag.TimeSlots = new List<string> { "09:00 - 11:00", "11:00 - 13:00", "13:00 - 15:00", "15:00 - 17:00" };

            return View(new CreateDeliveryScheduleDto());
        }

        [HttpPost("CreateDeliverySchedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeliverySchedule(CreateDeliveryScheduleDto model)
        {
            if (!ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _context.Bookings
                    .Include(b => b.StorageUnit)
                    .Where(b => b.Client != null && b.Client.UserId != null && b.Client.UserId == userId
                        && b.Status != BookingStatus.Cancelled)
                    .Select(b => new BookingResponseDto
                    {
                        Id = b.Id,
                        BookingNumber = b.BookingNumber ?? string.Empty,
                        UnitNumber = b.StorageUnit != null ? (b.StorageUnit.UnitNumber ?? "Unknown") : "Unknown",
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount
                    })
                    .ToListAsync();
                ViewBag.Bookings = bookings;
                ViewBag.TimeSlots = new List<string> { "09:00 - 11:00", "11:00 - 13:00", "13:00 - 15:00", "15:00 - 17:00" };
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get the client
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
                if (client == null)
                {
                    TempData["Error"] = "Client not found";
                    return RedirectToAction("DeliverySchedule");
                }

                // Create the delivery schedule
                var schedule = new DeliverySchedule
                {
                    ScheduleNumber = $"DS-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    BookingId = model.BookingId,
                    ClientId = client.Id,
                    DeliveryType = Enum.Parse<DeliveryType>(model.DeliveryType),
                    ScheduledDate = model.ScheduledDate,
                    ScheduledTime = TimeSpan.FromHours(9),
                    TimeSlot = model.TimeSlot,
                    PickupAddress = model.PickupAddress ?? string.Empty,
                    DeliveryAddress = model.DeliveryAddress ?? string.Empty,
                    GoodsDescription = model.GoodsDescription ?? string.Empty,
                    ItemCount = model.ItemCount,
                    EstimatedWeight = model.EstimatedWeight,
                    Status = ScheduleStatus.Pending,
                    CreatedAt = DateTime.Now,
                    SpecialInstructions = model.SpecialInstructions ?? string.Empty,
                    ContactPerson = model.ContactPerson ?? string.Empty,
                    ContactPhone = model.ContactPhone ?? string.Empty
                };

                _context.DeliverySchedules.Add(schedule);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Delivery schedule {schedule.ScheduleNumber} created successfully!";
                return RedirectToAction("DeliverySchedule");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating schedule: {ex.Message}";
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _context.Bookings
                    .Include(b => b.StorageUnit)
                    .Where(b => b.Client != null && b.Client.UserId != null && b.Client.UserId == userId
                        && b.Status != BookingStatus.Cancelled)
                    .Select(b => new BookingResponseDto
                    {
                        Id = b.Id,
                        BookingNumber = b.BookingNumber ?? string.Empty,
                        UnitNumber = b.StorageUnit != null ? (b.StorageUnit.UnitNumber ?? "Unknown") : "Unknown",
                        StartDate = b.StartDate,
                        EndDate = b.EndDate,
                        Status = b.Status.ToString(),
                        TotalAmount = b.TotalAmount
                    })
                    .ToListAsync();
                ViewBag.Bookings = bookings;
                ViewBag.TimeSlots = new List<string> { "09:00 - 11:00", "11:00 - 13:00", "13:00 - 15:00", "15:00 - 17:00" };
                return View(model);
            }
        }

        [HttpGet("DeliveryScheduleDetails/{id}")]
        public async Task<IActionResult> DeliveryScheduleDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Id == id && d.Booking.Client.UserId == userId)
                .Select(d => new DeliveryScheduleResponseDto
                {
                    Id = d.Id,
                    ScheduleNumber = d.ScheduleNumber ?? string.Empty,
                    ScheduledDate = d.ScheduledDate,
                    TimeSlot = d.TimeSlot ?? string.Empty,
                    Status = d.Status.ToString(),
                    DeliveryType = d.DeliveryType.ToString(),
                    UnitNumber = d.Booking != null && d.Booking.StorageUnit != null
                        ? (d.Booking.StorageUnit.UnitNumber ?? "Unknown")
                        : "Unknown",
                    ItemCount = d.ItemCount,
                    PickupAddress = d.PickupAddress ?? string.Empty,
                    DeliveryAddress = d.DeliveryAddress ?? string.Empty,
                    GoodsDescription = d.GoodsDescription ?? string.Empty,
                    EstimatedWeight = d.EstimatedWeight,
                    SpecialInstructions = d.SpecialInstructions ?? string.Empty,
                    ContactPerson = d.ContactPerson ?? string.Empty,
                    ContactPhone = d.ContactPhone ?? string.Empty,
                    CanReschedule = d.Status == ScheduleStatus.Pending || d.Status == ScheduleStatus.Confirmed,
                    CanSchedule = d.Status == ScheduleStatus.Pending
                })
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId != null && c.UserId == userId);
            if (client == null)
            {
                client = new Core.Entities.Client { UserId = userId };
            }

            var viewModel = new UpdateProfileViewModel
            {
                FullName = (client.FullName != null) ? client.FullName : string.Empty,
                PreferredName = (client.PreferredName != null) ? client.PreferredName : string.Empty,
                Email = (client.Email != null) ? client.Email : string.Empty,
                Phone = (client.Phone != null) ? client.Phone : string.Empty,
                IdNumber = (client.IdNumber != null) ? client.IdNumber : string.Empty,
                Address = (client.Address != null) ? client.Address : string.Empty
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

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId != null && c.UserId == userId);

            if (client == null)
            {
                client = new Core.Entities.Client { UserId = userId };
                _context.Clients.Add(client);
            }

            client.FullName = (model.FullName != null) ? model.FullName : string.Empty;
            client.PreferredName = (model.PreferredName != null) ? model.PreferredName : string.Empty;
            client.Email = (model.Email != null) ? model.Email : string.Empty;
            client.Phone = (model.Phone != null) ? model.Phone : string.Empty;
            client.IdNumber = (model.IdNumber != null) ? model.IdNumber : string.Empty;
            client.Address = (model.Address != null) ? model.Address : string.Empty;

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
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("PreferredName", (client.PreferredName != null) ? client.PreferredName : string.Empty));
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
                return NotFound("Unable to load user.");
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

        [HttpPost("CancelBooking")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.Client)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking != null && booking.Client != null && booking.Client.UserId != null && booking.Client.UserId == userId)
                {
                    booking.Status = BookingStatus.Cancelled;

                    var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
                    if (invoice != null)
                    {
                        invoice.Status = InvoiceStatus.Cancelled;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Booking {booking.BookingNumber} has been cancelled successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("MyBookings");
        }

        [HttpGet("MyStorage")]
        public async Task<IActionResult> MyStorage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Get contracts that are Accepted or Active
            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(c => c.Client != null && c.Client.UserId != null && c.Client.UserId == userId
                    && (c.Status == ContractStatus.Accepted || c.Status == ContractStatus.Active))
                .Select(c => new ContractResponseDto
                {
                    Id = c.Id,
                    ContractNumber = c.ContractNumber ?? string.Empty,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MonthlyRate = c.MonthlyRate,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt,
                    UnitNumber = c.Booking != null && c.Booking.StorageUnit != null
                        ? (c.Booking.StorageUnit.UnitNumber ?? "Unknown")
                        : "Unknown",
                    UnitSize = c.Booking != null && c.Booking.StorageUnit != null
                        ? (c.Booking.StorageUnit.Size ?? "Unknown")
                        : "Unknown"
                })
                .ToListAsync();

            return View(contracts);
        }

        [HttpGet("Payments")]
        public async Task<IActionResult> Payments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var invoices = await _context.Invoices
                .Where(i => i.Client != null && i.Client.UserId != null && i.Client.UserId == userId && i.Status == InvoiceStatus.Paid)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber ?? string.Empty,
                    Amount = i.Amount,
                    AmountPaid = i.AmountPaid,
                    Balance = i.Balance,
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString(),
                    PaymentDate = i.PaidAt,
                    PaymentMethod = i.PaymentMethod ?? string.Empty,
                    PaymentReference = i.PaymentReference ?? string.Empty
                })
                .OrderByDescending(i => i.PaymentDate)
                .ToListAsync();

            return View(invoices);
        }

        [HttpGet("PaymentHistory")]
        public async Task<IActionResult> PaymentHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var invoices = await _context.Invoices
                .Where(i => i.Client != null && i.Client.UserId != null && i.Client.UserId == userId)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber ?? string.Empty,
                    Amount = i.Amount,
                    AmountPaid = i.AmountPaid,
                    Balance = i.Balance,
                    InvoiceDate = i.InvoiceDate,
                    DueDate = i.DueDate,
                    Status = i.Status.ToString(),
                    PaymentDate = i.PaidAt,
                    PaymentMethod = i.PaymentMethod ?? string.Empty,
                    PaymentReference = i.PaymentReference ?? string.Empty
                })
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return View(invoices);
        }

        [HttpGet("Cartage")]
        public async Task<IActionResult> Cartage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedules = await _context.DeliverySchedules
                .Include(d => d.Booking)
                .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Booking != null && d.Booking.Client != null && d.Booking.Client.UserId != null && d.Booking.Client.UserId == userId)
                .Select(d => new DeliveryScheduleResponseDto
                {
                    Id = d.Id,
                    ScheduleNumber = d.ScheduleNumber ?? string.Empty,
                    ScheduledDate = d.ScheduledDate,
                    TimeSlot = d.TimeSlot ?? string.Empty,
                    Status = d.Status.ToString(),
                    DeliveryType = d.DeliveryType.ToString(),
                    BookingId = d.BookingId,
                    UnitNumber = d.Booking.StorageUnit != null ? (d.Booking.StorageUnit.UnitNumber ?? "Unknown") : "Unknown"
                })
                .OrderByDescending(d => d.ScheduledDate)
                .ToListAsync();

            return View(schedules);
        }

        [HttpGet("VehicleServices")]
        public IActionResult VehicleServices()
        {
            return View();
        }

        [HttpGet("RescheduleDelivery/{id}")]
        public async Task<IActionResult> RescheduleDelivery(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.Id == id && d.Booking.Client.UserId == userId);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Status != ScheduleStatus.Pending && schedule.Status != ScheduleStatus.Confirmed)
            {
                TempData["Error"] = "This delivery cannot be rescheduled.";
                return RedirectToAction("DeliverySchedule");
            }

            ViewBag.Schedule = schedule;
            ViewBag.TimeSlots = new List<string> { "09:00 - 11:00", "11:00 - 13:00", "13:00 - 15:00", "15:00 - 17:00" };

            return View();
        }

        [HttpPost("RescheduleDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleDelivery(int id, DateTime newDate, string timeSlot)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(d => d.Id == id && d.Booking.Client.UserId == userId);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Status != ScheduleStatus.Pending && schedule.Status != ScheduleStatus.Confirmed)
            {
                TempData["Error"] = "This delivery cannot be rescheduled.";
                return RedirectToAction("DeliverySchedule");
            }

            schedule.ScheduledDate = newDate;
            schedule.TimeSlot = timeSlot;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Delivery rescheduled to {newDate:dd MMM yyyy} at {timeSlot}";
            return RedirectToAction("DeliverySchedule");
        }

        [HttpPost("Customer/CancelDeliverySchedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelDeliverySchedule(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var schedule = await _context.DeliverySchedules
                .FirstOrDefaultAsync(d => d.Id == id && d.Booking.Client.UserId == userId);

            if (schedule == null)
            {
                TempData["Error"] = "Delivery schedule not found.";
                return RedirectToAction("DeliverySchedule");
            }

            if (schedule.Status != ScheduleStatus.Pending && schedule.Status != ScheduleStatus.Confirmed)
            {
                TempData["Error"] = "This delivery cannot be cancelled.";
                return RedirectToAction("DeliverySchedule");
            }

            schedule.Status = ScheduleStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Delivery schedule has been cancelled successfully.";
            return RedirectToAction("DeliverySchedule");
        }




    }
}

#if SupressWarnings
#pragma warning restore CS8602
#pragma warning restore CS0229
#endif