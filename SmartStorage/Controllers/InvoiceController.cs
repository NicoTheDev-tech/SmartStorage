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

            // Create contract for the booking
            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);
            if (contract == null)
            {
                contract = new Contract
                {
                    ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-{bookingId}",
                    BookingId = bookingId,
                    ClientId = booking.ClientId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    MonthlyRate = booking.StorageUnit?.MonthlyRate ?? 500,
                    SecurityDeposit = 50,
                    TotalContractValue = (booking.StorageUnit?.MonthlyRate ?? 500) * 3,
                    TermsAndConditions = GetTermsAndConditions(),
                    SpecialConditions = "",
                    Status = ContractStatus.PendingAcceptance,
                    CreatedAt = DateTime.Now
                };
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Payment successful! Please review and sign your contract.";
            return RedirectToAction("SignContract", new { bookingId = bookingId });
        }

        private string GetTermsAndConditions()
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
   - Deposit may be withheld for damages or unpaid fees

3. PROHIBITED ITEMS
   - Hazardous materials (chemicals, flammable liquids, explosives)
   - Perishable goods (food, plants, animals)
   - Illegal substances or stolen goods
   - Flammable or combustible materials

4. ACCESS AND SECURITY
   - 24/7 access with valid ID and access code
   - Customer is responsible for their own locks and security
   - Facility has 24/7 CCTV surveillance

5. CONTRACT EXTENSION
   - Customer may request a contract extension at least 30 days prior to the contract end date
   - Extension requests must be submitted in writing via email or customer portal
   - Extension is subject to unit availability and current market rates
   - A new contract will be issued for the extended period
   - Automatic month-to-month extension applies if no cancellation notice is received

6. DEFAULT AND ABANDONED PROPERTY
   - Failure to pay rental fees for 30 consecutive days constitutes default
   - Customer will receive written notice (email and SMS) of default status
   - After 60 days of non-payment and no communication from Customer, the account will be considered abandoned
   - SmartStorage reserves the right to take legal possession of abandoned property
   - Abandoned property may be sold, donated, or disposed of to recover outstanding fees
   - Any proceeds from sale of goods will first be applied to outstanding fees
   - Remaining balance, if any, will be held for the Customer for 90 days
   - Customer acknowledges that valuable or sentimental items should not be stored without adequate insurance

7. INSURANCE
   - Customer is strongly advised to maintain comprehensive insurance for stored items
   - SmartStorage is not liable for loss, damage, or theft of stored items
   - Customer assumes all risk for stored belongings
   - SmartStorage facility insurance does not cover Customer's stored property

8. TERMINATION
   - 30 days written notice required for contract termination
   - Early termination may result in forfeiture of deposit
   - All outstanding fees must be settled before property release

I have read, understood, and agree to the above terms and conditions.
";
        }

        [HttpGet("Invoice/SignContract/{bookingId}")]
        public async Task<IActionResult> SignContract(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings
                .Include(b => b.StorageUnit)
                .Include(b => b.Client)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.Client.UserId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);

            if (contract == null)
            {
                contract = new Contract
                {
                    ContractNumber = $"CT-{DateTime.Now:yyyyMMdd}-{bookingId}",
                    BookingId = bookingId,
                    ClientId = booking.ClientId,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    MonthlyRate = booking.StorageUnit?.MonthlyRate ?? 500,
                    SecurityDeposit = 50,
                    TotalContractValue = (booking.StorageUnit?.MonthlyRate ?? 500) * 3,
                    TermsAndConditions = GetTermsAndConditions(),
                    SpecialConditions = "",
                    Status = ContractStatus.PendingAcceptance,
                    CreatedAt = DateTime.Now
                };
                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
            }

            ViewBag.Booking = booking;
            ViewBag.Contract = contract;
            return View();
        }

        [HttpPost("Invoice/SignContract")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignContractPost(int bookingId, string signatureName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _context.Bookings.FindAsync(bookingId);

            if (booking == null)
            {
                return Json(new { success = false, error = "Booking not found" });
            }

            var contract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);
            if (contract == null)
            {
                return Json(new { success = false, error = "Contract not found" });
            }

            contract.Status = ContractStatus.Accepted;
            contract.AcceptedAt = DateTime.Now;
            contract.AcceptedBy = signatureName;
            contract.AcceptedIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _context.SaveChangesAsync();

            return Json(new { success = true, bookingId = bookingId });
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