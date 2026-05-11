#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using SmartStorage.Infrastructure.Data;
using SmartStorage.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    public class ReserveController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;
        private readonly IDeliveryScheduleService _deliveryService;
        private readonly ILogger<ReserveController> _logger;

        public ReserveController(
            IBookingService bookingService,
            ApplicationDbContext context,
            IDeliveryScheduleService deliveryService,
            ILogger<ReserveController> logger)
        {
            _bookingService = bookingService;
            _context = context;
            _deliveryService = deliveryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ReserveWizardViewModel
            {
                StorageTypes = new System.Collections.Generic.List<StorageTypeOption>
                {
                    new StorageTypeOption { Value = "self", Name = "Self Storage Unit", Description = "Standard individual unit. You load and unload yourself.", Icon = "bi-box-seam" },
                    new StorageTypeOption { Value = "climate", Name = "Climate-Controlled Unit", Description = "Temperature and humidity regulated. Ideal for electronics, documents, wine, antiques.", Icon = "bi-thermometer-snow" },
                    new StorageTypeOption { Value = "vehicle", Name = "Vehicle Storage", Description = "Parking spaces for cars, RVs, boats, or motorcycles.", Icon = "bi-car-front" },
                    new StorageTypeOption { Value = "business", Name = "Business Storage", Description = "Commercial units with extended access hours. Ideal for inventory or equipment.", Icon = "bi-building" },
                    new StorageTypeOption { Value = "household", Name = "Household Storage", Description = "Perfect for furniture, boxes, and personal belongings.", Icon = "bi-house-door" }
                }
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId != null && c.UserId == userId);
                if (client != null)
                {
                    viewModel.PickupAddress = client.Address ?? string.Empty;
                    viewModel.DeliveryAddress = client.Address ?? string.Empty;
                }
            }

            return View(viewModel);
        }

        // UPDATED: Removed minPrice and maxPrice parameters
        public async Task<IActionResult> GetAvailableUnits(string storageType, string size, string location, bool climateControlled)
        {
            var units = await _context.StorageUnits.Where(u => u.IsActive).ToListAsync();
            var result = new System.Collections.Generic.List<AvailableUnit>();

            foreach (var unit in units)
            {
                string sizeCategory = "Medium";
                if (unit.Size == "5x5") sizeCategory = "Small";
                else if (unit.Size == "10x10") sizeCategory = "Medium";
                else if (unit.Size == "10x20") sizeCategory = "Large";
                else if (unit.Size == "20x20") sizeCategory = "ExtraLarge";
                else if (unit.Size == "Vehicle Bay" || unit.Size == "Covered Vehicle") sizeCategory = "Small";
                else if (unit.Size == "Boat Storage") sizeCategory = "Large";
                else if (unit.Size == "RV Storage") sizeCategory = "ExtraLarge";

                if (!string.IsNullOrEmpty(size) && sizeCategory != size) continue;
                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(unit.Location) && !unit.Location.Contains(location)) continue;
                if (climateControlled && (unit.ClimateControl != "Basic" && unit.ClimateControl != "Premium")) continue;

                result.Add(new AvailableUnit
                {
                    Id = unit.Id,
                    UnitNumber = unit.UnitNumber ?? string.Empty,
                    Size = unit.Size ?? sizeCategory,
                    SizeCategory = sizeCategory,
                    Floor = GetFloorFromLocation(unit.Location),
                    Features = GetFeatureDescription(unit.ClimateControl ?? "Standard"),
                    Price = unit.MonthlyRate,
                    Description = GetSizeDescription(storageType, sizeCategory)
                });
            }

            return Json(result);
        }

        private string GetFloorFromLocation(string? location)
        {
            if (string.IsNullOrEmpty(location)) return "Ground Floor";
            if (location.Contains("Westville")) return "Ground Floor";
            if (location.Contains("Pinetown")) return "Ground Floor";
            if (location.Contains("Umhlanga")) return "First Floor";
            if (location.Contains("Durban")) return "Ground Floor";
            return "Ground Floor";
        }

        private string GetFeatureDescription(string climateControl)
        {
            if (string.IsNullOrEmpty(climateControl) || climateControl == "None")
                return "✓ 24/7 access | ✓ Security cameras | ✓ On-site manager";
            if (climateControl == "Basic")
                return "✓ Basic temperature control | ✓ Security cameras | ✓ Drive-up access";
            if (climateControl == "Premium")
                return "✓ Full climate control | ✓ Humidity regulated | ✓ 24/7 Security | ✓ Individual alarm";
            return "✓ 24/7 access | ✓ Security cameras | ✓ On-site manager";
        }

        private string GetSizeDescription(string storageType, string sizeCategory)
        {
            if (storageType == "vehicle")
            {
                return sizeCategory == "Small" ? "Small Car (Sedan, Hatchback)" :
                       sizeCategory == "Medium" ? "SUV / Light Truck" :
                       sizeCategory == "Large" ? "Large Truck / Small Boat" :
                       "Boat / RV / Caravan";
            }
            return sizeCategory == "Small" ? "Up to 50 items / 1 room" :
                   sizeCategory == "Medium" ? "1-bedroom apartment / 100 items" :
                   sizeCategory == "Large" ? "2-3 bedroom house / Furniture" :
                   "Full house / Business stock";
        }

        public async Task<IActionResult> CheckAvailability(int unitId, DateTime startDate, DateTime endDate)
        {
            var isAvailable = await _bookingService.CheckAvailability(unitId, startDate, endDate);
            return Json(new { available = isAvailable });
        }

        public async Task<IActionResult> Create([FromBody] CreateBookingDto bookingDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookingResponse = await _bookingService.CreateBooking(bookingDto, userId ?? string.Empty);

                // Get the actual Booking entity from database to update its status
                var bookingEntity = await _context.Bookings.FindAsync(bookingResponse.Id);

                if (bookingEntity != null)
                {
                    // Set status to Pending (not Confirmed)
                    bookingEntity.Status = BookingStatus.Pending;
                    await _context.SaveChangesAsync();
                }

                bool emailSent = false;

                try
                {
                    var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId != null && c.UserId == userId);

                    if (client != null && !string.IsNullOrEmpty(client.Email) && bookingEntity != null)
                    {
                        await emailService.SendBookingPendingEmailAsync(bookingEntity, client.Email, client.FullName);
                        emailSent = true;
                        _logger.LogInformation($"Booking pending email sent to {client.Email} for booking {bookingResponse.BookingNumber}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send booking pending email");
                }

                return Json(new
                {
                    success = true,
                    booking = bookingResponse,
                    emailSent = emailSent,
                    message = "✓ Booking created! Please review and sign your contract to complete payment."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // NEW: Contract signing page
        [HttpGet("Reserve/Contract/{bookingId}")]
        public async Task<IActionResult> Contract(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.Client.UserId == userId);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost("Reserve/SignContract")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignContract(int bookingId, string signatureName)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, error = "User not logged in" });
                }

                var booking = await _context.Bookings
                    .Include(b => b.Client)
                    .Include(b => b.StorageUnit)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return Json(new { success = false, error = "Booking not found" });
                }

                // Verify the booking belongs to the logged-in user
                if (booking.Client == null || booking.Client.UserId != userId)
                {
                    return Json(new { success = false, error = "You don't have permission to sign this contract" });
                }

                // Check if contract already exists
                var existingContract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);

                if (existingContract == null)
                {
                    var contract = new Contract
                    {
                        ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-{bookingId}",
                        BookingId = bookingId,
                        ClientId = booking.ClientId,
                        StartDate = booking.StartDate,
                        EndDate = booking.EndDate,
                        MonthlyRate = booking.StorageUnit?.MonthlyRate ?? 500,
                        SecurityDeposit = 50,
                        TotalContractValue = (booking.StorageUnit?.MonthlyRate ?? 500) * 3,
                        TermsAndConditions = GetContractTerms(),
                        SpecialConditions = "",
                        Status = ContractStatus.PendingAcceptance,
                        CreatedAt = DateTime.Now,
                        AcceptedBy = signatureName,
                        AcceptedAt = DateTime.Now
                    };
                    _context.Contracts.Add(contract);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, bookingId = bookingId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GetContractTerms()
        {
            return @"
SMARTSTORAGE STORAGE CONTRACT TERMS AND CONDITIONS

1. STORAGE UNIT RENTAL
   - The Customer agrees to rent the storage unit for the agreed period
   - Monthly rental fees are payable in advance on the 1st of each month
   - A 10% late fee will be applied to payments received after the 5th of the month

2. SECURITY DEPOSIT
   - A security deposit of R50 is required
   - Deposit is refundable upon contract termination with 30 days written notice

3. PROHIBITED ITEMS
   - Hazardous materials, perishable goods, illegal substances, flammable materials

4. ACCESS AND SECURITY
   - 24/7 access with valid ID and access code
   - Customer is responsible for their own locks and security

5. CONTRACT EXTENSION
   - Customer may request a contract extension at least 30 days prior to the contract end date

6. DEFAULT AND ABANDONED PROPERTY
   - Failure to pay rental fees for 30 consecutive days constitutes default
   - After 60 days of non-payment, property may be sold to recover outstanding fees

7. INSURANCE
   - Customer is strongly advised to maintain comprehensive insurance for stored items
   - SmartStorage is not liable for loss, damage, or theft of stored items

8. TERMINATION
   - 30 days written notice required for contract termination

I have read, understood, and agree to the above terms and conditions.
";
        }

        public async Task<IActionResult> Success(int id)
        {
            var booking = await _bookingService.GetBookingById(id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public async Task<IActionResult> CreateSchedule([FromBody] CreateDeliveryScheduleDto scheduleDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.CreateSchedule(scheduleDto, userId ?? string.Empty);
                return Json(new { success = true, schedule });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
#nullable restore