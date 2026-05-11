using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Core.Entities;
using System.Security.Claims;
using SmartStorage.Core.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartStorage.Controllers
{
    [Authorize(Roles = "Admin,OperationsManager")]
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

        [HttpGet("PendingExtensions")]
        [HttpGet("Admin/PendingExtensions")]
        public async Task<IActionResult> PendingExtensions()
        {
            var pendingExtensions = await _context.ContractExtensions
                .Include(e => e.Contract)
                    .ThenInclude(c => c.Client)
                .Include(e => e.Contract)
                    .ThenInclude(c => c.Booking)
                        .ThenInclude(b => b.StorageUnit)
                .Where(e => e.Status == "Pending")
                .OrderBy(e => e.RequestedDate)
                .ToListAsync();

            return View(pendingExtensions);
        }

        [HttpPost("ApproveExtension/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveExtension(int id, string? adminNotes)
        {
            var extension = await _context.ContractExtensions
                .Include(e => e.Contract)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extension == null)
            {
                return NotFound();
            }

            extension.Status = "Approved";
            extension.AdminNotes = adminNotes;
            extension.ApprovedDate = DateTime.Now;
            extension.ApprovedBy = User.Identity?.Name;

            if (extension.Contract != null)
            {
                extension.Contract.EndDate = extension.ProposedNewEndDate;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Extension approved successfully!";
            return RedirectToAction("PendingExtensions");
        }

        [HttpPost("RejectExtension/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectExtension(int id, string adminNotes)
        {
            var extension = await _context.ContractExtensions.FindAsync(id);

            if (extension == null)
            {
                return NotFound();
            }

            extension.Status = "Rejected";
            extension.AdminNotes = adminNotes;
            extension.ApprovedDate = DateTime.Now;
            extension.ApprovedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Extension rejected.";
            return RedirectToAction("PendingExtensions");
        }

        [HttpGet("Admin/GetDrivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await _context.Drivers
                .Where(d => d.Status == StaffStatus.Active)
                .Select(d => new { d.Id, d.FullName, d.VehicleAssigned, d.Phone })
                .ToListAsync();

            return Json(drivers);
        }

        [HttpPost("Admin/AssignDriverToDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriverToDelivery(int deliveryId, int driverId)
        {
            var delivery = await _context.DeliverySchedules.FindAsync(deliveryId);
            var driver = await _context.Drivers.FindAsync(driverId);

            if (delivery == null || driver == null)
            {
                return Json(new { success = false, message = "Delivery or driver not found" });
            }

            delivery.AssignedDriverId = driverId;
            delivery.Status = ScheduleStatus.Confirmed;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Driver {driver.FullName} assigned to delivery {delivery.ScheduleNumber}" });
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
                    Status = d.Status.ToString(),
                    AssignedDriver = d.AssignedDriver != null ? d.AssignedDriver.FullName : string.Empty
                })
                .ToListAsync();

            // Add drivers to ViewBag
            var drivers = await _context.Drivers
                .Where(d => d.Status == StaffStatus.Active)
                .Select(d => new { d.Id, d.FullName, d.VehicleAssigned })
                .ToListAsync();

            ViewBag.Drivers = drivers;

            return View(schedules);
        }

        // ============ BREACH CASE METHODS ============

        [HttpGet("Admin/BreachCases")]
        public async Task<IActionResult> BreachCases()
        {
            var breachCases = await _context.BreachCases
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(breachCases);
        }

        [HttpGet("Admin/CreateBreachCase")]
        public IActionResult CreateBreachCase()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBreachCase(string customerName, int contractId, string breachReason, decimal outstandingAmount)
        {
            var breachCase = new BreachCase
            {
                ContractId = contractId,
                CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "admin",
                CustomerName = customerName,
                BreachReason = breachReason,
                BreachDate = DateTime.Now,
                OutstandingAmount = outstandingAmount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.BreachCases.Add(breachCase);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Breach case for {customerName} created successfully!";
            return RedirectToAction("BreachCases");
        }

        [HttpGet("Admin/ApproveBreachCase/{id}")]
        public async Task<IActionResult> ApproveBreachCase(int id)
        {
            var breachCase = await _context.BreachCases.FindAsync(id);
            if (breachCase != null)
            {
                breachCase.Status = "Approved";
                breachCase.ApprovedDate = DateTime.Now;
                breachCase.ApprovedBy = User.Identity?.Name;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Breach case approved.";
            }
            return RedirectToAction("BreachCases");
        }

        [HttpGet("Admin/RejectBreachCase/{id}")]
        public async Task<IActionResult> RejectBreachCase(int id)
        {
            var breachCase = await _context.BreachCases.FindAsync(id);
            if (breachCase != null)
            {
                breachCase.Status = "Rejected";
                breachCase.ApprovedDate = DateTime.Now;
                breachCase.ApprovedBy = User.Identity?.Name;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Breach case rejected.";
            }
            return RedirectToAction("BreachCases");
        }

        [HttpGet("Admin/CatalogueGoods/{breachCaseId}")]
        public async Task<IActionResult> CatalogueGoods(int breachCaseId)
        {
            var breachCase = await _context.BreachCases.FindAsync(breachCaseId);
            if (breachCase == null || breachCase.Status != "Approved")
            {
                TempData["Error"] = "Only approved breach cases can be catalogued";
                return RedirectToAction("BreachCases");
            }

            ViewBag.BreachCaseId = breachCaseId;
            ViewBag.CustomerName = breachCase.CustomerName;
            return View();
        }

        [HttpPost("Admin/CatalogueGoods")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CatalogueGoods(AssetForSale asset)
        {
            if (ModelState.IsValid)
            {
                asset.Status = "Catalogued";
                asset.CreatedAt = DateTime.Now;
                _context.AssetsForSale.Add(asset);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Item '{asset.ItemName}' has been catalogued!";

                // Redirect to ViewCatalogue page directly
                return RedirectToAction("ViewCatalogue", new { breachCaseId = asset.BreachCaseId });
            }

            // If error, go back to catalogue form
            ViewBag.BreachCaseId = asset.BreachCaseId;
            return View(asset);
        }

        [HttpGet("Admin/ViewCatalogue/{breachCaseId}")]
        public async Task<IActionResult> ViewCatalogue(int breachCaseId)
        {
            var assets = await _context.AssetsForSale
                .Where(a => a.BreachCaseId == breachCaseId)
                .ToListAsync();

            var breachCase = await _context.BreachCases.FindAsync(breachCaseId);
            ViewBag.CustomerName = breachCase?.CustomerName;
            ViewBag.BreachCaseId = breachCaseId;

            return View(assets);
        }

        [HttpPost("Admin/DeleteAsset/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var asset = await _context.AssetsForSale.FindAsync(id);
            if (asset != null)
            {
                int breachCaseId = asset.BreachCaseId;
                _context.AssetsForSale.Remove(asset);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from catalogue";
                return RedirectToAction("ViewCatalogue", new { breachCaseId = breachCaseId });
            }
            return RedirectToAction("BreachCases");
        }

        [HttpGet("Admin/CreateAuction/{breachCaseId}")]
        public async Task<IActionResult> CreateAuction(int breachCaseId)
        {
            var assets = await _context.AssetsForSale
                .Where(a => a.BreachCaseId == breachCaseId)
                .ToListAsync();

            if (!assets.Any())
            {
                TempData["Error"] = "No catalogued goods found. Please add items to catalogue first.";
                return RedirectToAction("ViewCatalogue", new { breachCaseId = breachCaseId });
            }

            ViewBag.Assets = assets;
            ViewBag.BreachCaseId = breachCaseId;
            return View();
        }

        [HttpPost("Admin/CreateAuction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAuction(int breachCaseId, DateTime endDate, decimal startingBid)
        {
            // Get all catalogued assets for this breach case
            var assets = await _context.AssetsForSale
                .Where(a => a.BreachCaseId == breachCaseId && a.Status == "Catalogued")
                .ToListAsync();

            if (!assets.Any())
            {
                TempData["Error"] = "No catalogued goods found.";
                return RedirectToAction("BreachCases");
            }

            // Create the auction
            var auction = new Auction
            {
                AuctionNumber = "AUC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                BreachCaseId = breachCaseId,
                StartDate = DateTime.Now,
                EndDate = endDate,
                StartingBid = startingBid,
                Status = "Published",
                CreatedAt = DateTime.Now
            };

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();

            // IMPORTANT: Create AuctionItems for each asset
            foreach (var asset in assets)
            {
                var auctionItem = new AuctionItem
                {
                    AuctionId = auction.Id,
                    AssetId = asset.Id,
                    ItemName = asset.ItemName,
                    ReservePrice = asset.ReservePrice,
                    Status = "Active"
                };
                _context.AuctionItems.Add(auctionItem);

                // Update asset status to Listed
                asset.Status = "Listed";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Auction {auction.AuctionNumber} has been published with {assets.Count} items!";
            return RedirectToAction("ViewAuction", new { id = auction.Id });
        }

        [HttpGet("Admin/ViewAuction/{id}")]
        public async Task<IActionResult> ViewAuction(int id)
        {
            var auction = await _context.Auctions
                .FirstOrDefaultAsync(a => a.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            var items = await _context.AuctionItems
                .Where(i => i.AuctionId == id)
                .ToListAsync();

            var bids = await _context.AuctionBids
                .Where(b => b.AuctionId == id)
                .OrderByDescending(b => b.BidAmount)
                .ToListAsync();

            ViewBag.Items = items;
            ViewBag.Bids = bids;

            return View(auction);
        }

        [HttpGet("Admin/AuctionBids/{auctionId}")]
        public async Task<IActionResult> AuctionBids(int auctionId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);
            if (auction == null)
            {
                return NotFound();
            }

            var bids = await _context.AuctionBids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.BidAmount)
                .ToListAsync();

            ViewBag.Auction = auction;
            return View(bids);
        }

        [HttpGet("Admin/AllAuctions")]
        public async Task<IActionResult> AllAuctions()
        {
            var auctions = await _context.Auctions
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(auctions);
        }

        [HttpPost("Admin/CloseAuction/{auctionId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseAuction(int auctionId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);

            if (auction == null)
            {
                TempData["Error"] = "Auction not found.";
                return RedirectToAction("AllAuctions");
            }

            if (auction.EndDate > DateTime.Now)
            {
                TempData["Error"] = "Auction has not ended yet. Cannot close before end date.";
                return RedirectToAction("AllAuctions");
            }

            if (auction.Status == "Closed")
            {
                TempData["Error"] = "Auction is already closed.";
                return RedirectToAction("AllAuctions");
            }

            // Get highest valid bid
            var highestBid = await _context.AuctionBids
                .Where(b => b.AuctionId == auctionId && b.Status == "Active")
                .OrderByDescending(b => b.BidAmount)
                .FirstOrDefaultAsync();

            if (highestBid != null)
            {
                auction.WinningBidId = highestBid.Id;
                auction.WinnerId = highestBid.BidderId;
                auction.Status = "Closed";

                // Mark winning bid as winner
                highestBid.Status = "Won";

                // Mark other bids as lost
                var otherBids = await _context.AuctionBids
                    .Where(b => b.AuctionId == auctionId && b.Id != highestBid.Id)
                    .ToListAsync();

                foreach (var bid in otherBids)
                {
                    bid.Status = "Lost";
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Auction closed! Winner: {highestBid.BidderName} with bid of R {highestBid.BidAmount:N2}";
            }
            else
            {
                auction.Status = "Closed";
                await _context.SaveChangesAsync();
                TempData["Info"] = "Auction closed with no bids.";
            }

            return RedirectToAction("AllAuctions");
        }

        [HttpGet("Admin/ScheduleTransport")]
        public async Task<IActionResult> ScheduleTransport()
        {
            // Get pending delivery requests that need scheduling
            var pendingDeliveries = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.Booking)
                    .ThenInclude(b => b.StorageUnit)
                .Where(d => d.Status == ScheduleStatus.Pending)
                .OrderBy(d => d.ScheduledDate)
                .ToListAsync();

            ViewBag.TimeSlots = new List<string> {
        "09:00 - 11:00",
        "11:00 - 13:00",
        "13:00 - 15:00",
        "15:00 - 17:00"
    };

            return View(pendingDeliveries);
        }

        [HttpPost("Admin/ScheduleTransport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleTransport(int scheduleId, DateTime scheduledDate, string timeSlot, string route, string notes)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(scheduleId);

            if (schedule == null)
            {
                TempData["Error"] = "Delivery schedule not found.";
                return RedirectToAction("ScheduleTransport");
            }

            schedule.ScheduledDate = scheduledDate;
            schedule.TimeSlot = timeSlot;
            schedule.Route = route;
            schedule.AdminNotes = notes;
            schedule.Status = ScheduleStatus.Confirmed;  // THIS IS KEY - Set to Confirmed (1)

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Transport job for {schedule.ScheduleNumber} has been scheduled for {scheduledDate:dd MMM yyyy} at {timeSlot}";
            return RedirectToAction("ScheduleTransport");
        }

        [HttpGet("Admin/AssignDriver/{scheduleId}")]
        public async Task<IActionResult> AssignDriver(int scheduleId)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .FirstOrDefaultAsync(d => d.Id == scheduleId && d.Status == ScheduleStatus.Confirmed);

            if (schedule == null)
            {
                TempData["Error"] = "No confirmed transport job found. Please schedule the job first.";
                return RedirectToAction("ScheduleTransport");
            }

            var availableDrivers = await _context.Drivers
                .Where(d => d.Status == StaffStatus.Active)
                .Select(d => new { d.Id, d.FullName, d.VehicleAssigned, d.Phone })
                .ToListAsync();

            ViewBag.Schedule = schedule;
            ViewBag.Drivers = availableDrivers;

            return View();
        }

        [HttpPost("Admin/AssignDriver")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int scheduleId, int driverId)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(scheduleId);
            var driver = await _context.Drivers.FindAsync(driverId);

            if (schedule == null)
            {
                TempData["Error"] = "Delivery schedule not found.";
                return RedirectToAction("ScheduleTransport");
            }

            if (driver == null)
            {
                TempData["Error"] = "Driver not found.";
                return RedirectToAction("AssignDriver", new { scheduleId = scheduleId });
            }

            schedule.AssignedDriverId = driverId;
            schedule.Status = ScheduleStatus.InProgress;  // Change to In Progress

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {driver.FullName} has been assigned to transport job {schedule.ScheduleNumber}";

            // Redirect to a confirmation page or back to Assign Driver with success message
            return RedirectToAction("AssignDriverConfirmation", new { scheduleId = scheduleId });
        }

        [HttpGet("Admin/AssignDriverConfirmation/{scheduleId}")]
        public async Task<IActionResult> AssignDriverConfirmation(int scheduleId)
        {
            var schedule = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Include(d => d.AssignedDriver)
                .FirstOrDefaultAsync(d => d.Id == scheduleId);

            if (schedule == null)
            {
                return RedirectToAction("ScheduleTransport");
            }

            return View(schedule);
        }

        [HttpGet("Admin/CustomerRatings")]
        public async Task<IActionResult> CustomerRatings()
        {
            var ratings = await _context.DeliverySchedules
                .Include(d => d.Booking)
                    .ThenInclude(b => b.Client)
                .Where(d => d.IsRated == true)
                .OrderByDescending(d => d.RatedAt)
                .Select(d => new
                {
                    d.ScheduleNumber,
                    CustomerName = d.Booking.Client.FullName,
                    d.Rating,
                    d.CustomerFeedback,
                    d.IssueReported,
                    d.RatedAt
                })
                .ToListAsync();

            return View(ratings);
        }
    }
}