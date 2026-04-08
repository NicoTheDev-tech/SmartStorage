using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SmartStorage.Controllers
{
    [Authorize]
    public class ContractController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public ContractController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        [HttpGet("Contracts/Create/{bookingId}")]
        public async Task<IActionResult> Create(int bookingId)
        {
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null) return NotFound();

            var existingContract = await _context.Contracts.FirstOrDefaultAsync(c => c.BookingId == bookingId);
            if (existingContract != null)
            {
                return RedirectToAction("View", new { id = existingContract.Id });
            }

            ViewBag.Booking = booking;
            ViewBag.Terms = GetFullTerms(); // Show terms BEFORE signing
            return View();
        }

        [HttpPost("Contracts/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int bookingId, bool acceptTerms)
        {
            if (!acceptTerms)
            {
                TempData["Error"] = "You must accept the terms and conditions";
                return RedirectToAction("Create", new { bookingId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var booking = await _bookingService.GetBookingById(bookingId);
            if (booking == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);

            var contract = new Contract
            {
                ContractNumber = $"CTR-{DateTime.Now:yyyyMMdd}-{bookingId}",
                BookingId = bookingId,
                ClientId = client?.Id ?? 1,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                MonthlyRate = booking.TotalAmount / 3,
                SecurityDeposit = 50,
                TotalContractValue = booking.TotalAmount,
                TermsAndConditions = GetFullTerms(),
                SpecialConditions = "",
                Status = ContractStatus.Active,
                CreatedAt = DateTime.Now,
                AcceptedAt = DateTime.Now,
                AcceptedBy = client?.FullName ?? "Customer"
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            await _bookingService.UpdateBookingStatus(bookingId, "Active");

            TempData["Success"] = "Contract created successfully!";
            return RedirectToAction("View", new { id = contract.Id });
        }

        [HttpGet("Contracts/View/{id}")]
        public async Task<IActionResult> View(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return NotFound();

            var booking = contract.Booking;
            var storageUnit = booking?.StorageUnit;

            ViewBag.UnitNumber = storageUnit?.UnitNumber ?? "N/A";
            ViewBag.Contract = contract;

            return View(contract);
        }

        private string GetFullTerms()
        {
            return @"SMARTSTORAGE STORAGE CONTRACT TERMS AND CONDITIONS

1. STORAGE PERIOD: The storage period shall commence on the Start Date and terminate on the End Date.

2. PAYMENT TERMS: Monthly rental fees are payable in advance on the first day of each month.

3. SECURITY DEPOSIT: A security deposit of R50 is required and is refundable upon contract end, subject to inspection.

4. PROHIBITED ITEMS: Hazardous materials, perishables, illegal goods, and living items are strictly prohibited.

5. ACCESS: Client has 24/7 access using their personal access code.

6. LIABILITY: SmartStorage shall not be liable for any loss or damage to stored items.

7. GOVERNING LAW: This contract is governed by the laws of South Africa.

8. LATE PAYMENT: Should a client fail to pay the monthly rental fee within 5 days of the due date, a late fee of R10 will be applied. If payment is not received within 30 days, SmartStorage reserves the right to terminate the contract and dispose of the stored items in accordance with local laws.

9. HOLDOVER: Should a client fail to vacate the storage unit by the End Date, a holdover fee of R20 per day will be applied until the unit is vacated.

10. DAMAGE: Should a client cause damage to the storage unit, they will be responsible for the cost of repairs.

11. EARLY TERMINATION: Should a client seek to terminate the contract early, they must provide at least 30 days' written notice.

12. EXTENSION: Should a client seek to extend the contract, they must provide written notice at least 30 days before the End Date.

13. ABANDONMENT: Should a client be nowhere to be found at the storage unit for 60 consecutive days, SmartStorage reserves the right to terminate the contract.

14. AGREEMENT: By signing this contract, the client agrees to abide by all terms and conditions outlined herein.

I, the undersigned, acknowledge that I have read, understood, and agree to be bound by these terms and conditions.";
        }
    }
}