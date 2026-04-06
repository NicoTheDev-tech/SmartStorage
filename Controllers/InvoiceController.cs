using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public InvoiceController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpGet("Invoices/Pay/{bookingId}")]
        public async Task<IActionResult> Pay(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _bookingService.GetBookingById(bookingId);

            if (booking == null)
                return NotFound();

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
            if (invoice == null)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
                invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{bookingId}",
                    BookingId = bookingId,
                    ClientId = client?.Id ?? 1,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14),
                    Amount = booking.TotalAmount + 75,
                    AmountPaid = 0,
                    Balance = booking.TotalAmount + 75,
                    PeriodStart = booking.StartDate,
                    PeriodEnd = booking.EndDate,
                    BillingMonth = booking.StartDate.Month,
                    BillingYear = booking.StartDate.Year,
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.Now,
                    Notes = $"Booking #{booking.BookingNumber}"
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
            }

            ViewBag.Booking = booking;
            ViewBag.Invoice = invoice;
            return View();
        }

        [HttpPost("Invoices/ProcessPayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null)
                return NotFound();

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);

            await _bookingService.UpdateBookingStatus(bookingId, "Confirmed");

            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.AmountPaid = invoice.Amount;
                invoice.Balance = 0;
                invoice.PaidAt = DateTime.Now;
                invoice.PaymentMethod = "Credit Card";
                invoice.PaymentReference = $"PAY-{DateTime.Now:yyyyMMdd}-{bookingId}";
                await _context.SaveChangesAsync();
            }

            var payment = new Payment
            {
                PaymentReference = $"PAY-{DateTime.Now:yyyyMMdd}-{bookingId}",
                BookingId = bookingId,
                Amount = invoice?.Amount ?? (booking.TotalAmount + 75),
                PaymentDate = DateTime.Now,
                Method = PaymentMethod.CreditCard,
                Status = PaymentStatus.Completed,
                TransactionId = Guid.NewGuid().ToString()
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful!";
            return RedirectToAction("Success", new { bookingId });
        }

        [HttpGet("Invoices/Success/{bookingId}")]
        public async Task<IActionResult> Success(int bookingId)
        {
            var booking = await _bookingService.GetBookingById(bookingId);
            ViewBag.Booking = booking;
            return View();
        }

        [HttpGet("Invoices/View/{id}")]
        public async Task<IActionResult> ViewInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(i => i.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (invoice.ClientId != client?.Id && !User.IsInRole("Admin"))
                return Forbid();

            ViewBag.Invoice = invoice;
            return View();
        }
    }
}