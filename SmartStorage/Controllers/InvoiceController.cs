using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Core.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            var invoices = _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Client)
                .AsQueryable();

            if (!User.IsInRole("Admin") && client != null)
            {
                invoices = invoices.Where(i => i.ClientId == client.Id);
            }

            return View(await invoices.OrderByDescending(i => i.InvoiceDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking)
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet("Invoice/Pay/{bookingId}")]
        public async Task<IActionResult> Pay(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.StorageUnit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);

            // If invoice doesn't exist, create one
            if (invoice == null)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

                var totalAmount = booking.TotalAmount;
                if (totalAmount <= 0 && booking.StorageUnit != null)
                {
                    var months = ((booking.EndDate.Year - booking.StartDate.Year) * 12) + (booking.EndDate.Month - booking.StartDate.Month);
                    months = months < 1 ? 1 : months;
                    totalAmount = booking.StorageUnit.MonthlyRate * months;
                }

                invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{bookingId}",
                    BookingId = bookingId,
                    ClientId = client?.Id ?? 1,
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14),
                    Amount = totalAmount,
                    AmountPaid = 0,
                    Balance = totalAmount,
                    PeriodStart = booking.StartDate,
                    PeriodEnd = booking.EndDate,
                    Status = InvoiceStatus.Pending,
                    CreatedAt = DateTime.Now,
                    Notes = $"Booking #{booking.BookingNumber}"
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                TempData["Error"] = "Cannot pay for a cancelled invoice.";
                return RedirectToAction("Index");
            }

            // Pass both objects to the view
            ViewBag.Booking = booking;
            ViewBag.Invoice = invoice;
            return View();
        }

        [HttpGet("Invoices/Pay/{bookingId}")]
        public async Task<IActionResult> PayPlural(int bookingId)
        {
            return RedirectToAction("Pay", new { bookingId });
        }

        [HttpPost("Invoice/ProcessPayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);

            if (invoice != null && invoice.Status == InvoiceStatus.Cancelled)
            {
                TempData["Error"] = "Cannot pay for a cancelled invoice.";
                return RedirectToAction("Index");
            }

            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.AmountPaid = invoice.Amount;
                invoice.Balance = 0;
                invoice.PaidAt = DateTime.Now;
                invoice.PaymentMethod = Request.Form["paymentMethod"].ToString();
                invoice.PaymentReference = $"PAY-{DateTime.Now:yyyyMMdd}-{bookingId}";
                await _context.SaveChangesAsync();
            }

            // Create payment record
            var payment = new Payment
            {
                PaymentReference = $"PAY-{DateTime.Now:yyyyMMdd}-{bookingId}",
                BookingId = bookingId,
                Amount = invoice?.Amount ?? booking.TotalAmount,
                PaymentDate = DateTime.Now,
                Method = PaymentMethod.CreditCard,
                Status = PaymentStatus.Completed,
                TransactionId = Guid.NewGuid().ToString()
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // UPDATE BOOKING STATUS TO CONFIRMED
            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            // Update contract status if exists
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);
            if (contract != null && contract.Status == ContractStatus.PendingAcceptance)
            {
                contract.Status = ContractStatus.Accepted;
                contract.AcceptedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Payment successful!";

            // Redirect to Success page (not SignContract)
            return RedirectToAction("Success", new { bookingId = bookingId });
        }

        [HttpGet("Invoice/Success/{bookingId}")]
        public async Task<IActionResult> Success(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.StorageUnit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.Booking = booking;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Invoice deleted successfully";
            }
            return RedirectToAction("Index");
        }
    }
}