using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    [Route("Delivery")]
    public class DeliveryController : Controller
    {
        private readonly IDeliveryScheduleService _deliveryService;
        private readonly IBookingService _bookingService;

        public DeliveryController(IDeliveryScheduleService deliveryService, IBookingService bookingService)
        {
            _deliveryService = deliveryService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var schedules = await _deliveryService.GetClientSchedules(userId ?? string.Empty);
            return View(schedules);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var schedule = await _deliveryService.GetScheduleById(id);
            if (schedule == null)
                return NotFound();

            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clientSchedules = await _deliveryService.GetClientSchedules(userId ?? string.Empty);
            if (!clientSchedules.Any(s => s.Id == id) && !User.IsInRole("Admin"))
                return Forbid();

            return View(schedule);
        }

        [HttpGet("Create/{bookingId}")]
        public async Task<IActionResult> Create(int bookingId)
        {
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null)
                return NotFound();

            ViewBag.Booking = booking;
            ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(DateTime.Today);
            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDeliveryScheduleDto createDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(createDto.ScheduledDate);
                return View(createDto);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.CreateSchedule(createDto, userId ?? string.Empty);
                TempData["Success"] = $"Delivery schedule {schedule.ScheduleNumber} created successfully!";
                return RedirectToAction("Details", new { id = schedule.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(createDto.ScheduledDate);
                return View(createDto);
            }
        }

        [HttpGet("Reschedule/{id}")]
        public async Task<IActionResult> Reschedule(int id)
        {
            var schedule = await _deliveryService.GetScheduleById(id);
            if (schedule == null)
                return NotFound();

            if (!schedule.CanReschedule)
            {
                TempData["Error"] = "This schedule cannot be rescheduled";
                return RedirectToAction("Details", new { id });
            }

            ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(schedule.ScheduledDate);
            return View(schedule);
        }

        [HttpPost("Reschedule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(UpdateDeliveryScheduleDto updateDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.UpdateSchedule(updateDto, userId ?? string.Empty);
                TempData["Success"] = $"Delivery schedule rescheduled to {schedule.ScheduledDate:dd MMM yyyy} at {schedule.TimeSlot}";
                return RedirectToAction("Details", new { id = schedule.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                var schedule = await _deliveryService.GetScheduleById(updateDto.ScheduleId);
                ViewBag.TimeSlots = await _deliveryService.GetAvailableTimeSlots(updateDto.ScheduledDate ?? schedule?.ScheduledDate ?? DateTime.Today);
                return View(schedule);
            }
        }

        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var schedule = await _deliveryService.CancelSchedule(id, userId ?? string.Empty);
                TempData["Success"] = "Delivery schedule cancelled successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }
    }
}