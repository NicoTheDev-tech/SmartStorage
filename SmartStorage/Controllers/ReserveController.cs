using Microsoft.AspNetCore.Authorization;
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
    public class ReserveController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;
        private readonly IDeliveryScheduleService _deliveryService;

        public ReserveController(IBookingService bookingService, ApplicationDbContext context, IDeliveryScheduleService deliveryService)
        {
            _bookingService = bookingService;
            _context = context;
            _deliveryService = deliveryService;
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
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
                if (client != null)
                {
                    viewModel.PickupAddress = client.Address ?? string.Empty;
                    viewModel.DeliveryAddress = client.Address ?? string.Empty;
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> GetAvailableUnits(string storageType, string size, string location, decimal? minPrice, decimal? maxPrice, bool climateControlled)
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
                if (minPrice.HasValue && unit.MonthlyRate < minPrice.Value) continue;
                if (maxPrice.HasValue && unit.MonthlyRate > maxPrice.Value) continue;
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
                var booking = await _bookingService.CreateBooking(bookingDto, userId ?? string.Empty);
                return Json(new { success = true, booking });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
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

        public IActionResult Test()
        {
            return Content("ReserveController is working!");
        }
    }
}