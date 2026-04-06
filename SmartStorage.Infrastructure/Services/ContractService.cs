using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Core.DTOs;
using SmartStorage.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartStorage.Infrastructure.Services
{
    public class ContractService : IContractService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractService> _logger;

        public ContractService(ApplicationDbContext context, ILogger<ContractService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ContractResponseDto> CreateContract(CreateContractDto createDto, string adminId)
        {
            if (createDto == null)
                throw new ArgumentNullException(nameof(createDto));

            if (string.IsNullOrEmpty(adminId))
                throw new ArgumentException("Admin ID is required", nameof(adminId));

            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .FirstOrDefaultAsync(b => b.Id == createDto.BookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Booking must be confirmed before creating contract");

            var existingContract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.BookingId == createDto.BookingId);

            if (existingContract != null)
                throw new InvalidOperationException("A contract already exists for this booking");

            var securityDeposit = createDto.MonthlyRate;

            var startDate = createDto.StartDate;
            var endDate = createDto.EndDate;
            var durationMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
            if (durationMonths <= 0) durationMonths = 1;

            var totalValue = (createDto.MonthlyRate * durationMonths) + securityDeposit;

            var contract = new Contract
            {
                ContractNumber = GenerateContractNumber(),
                BookingId = createDto.BookingId,
                ClientId = booking.ClientId,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                MonthlyRate = createDto.MonthlyRate,
                SecurityDeposit = securityDeposit,
                TotalContractValue = totalValue,
                TermsAndConditions = GetStandardTerms(),
                SpecialConditions = createDto.SpecialConditions ?? string.Empty,
                Status = ContractStatus.PendingAcceptance,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var result = await GetContractById(contract.Id);
            return result ?? throw new InvalidOperationException("Failed to retrieve created contract");
        }

        public async Task<ContractResponseDto> GenerateContractFromBooking(int bookingId, string adminId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.StorageUnit)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            var storageUnit = booking.StorageUnit;
            var monthlyRate = storageUnit?.MonthlyRate ?? 0;

            var createDto = new CreateContractDto
            {
                BookingId = bookingId,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                MonthlyRate = monthlyRate,
                SecurityDeposit = monthlyRate
            };

            return await CreateContract(createDto, adminId);
        }

        public async Task<ContractResponseDto> AcceptContract(AcceptContractDto acceptDto, string userId, string ipAddress)
        {
            if (acceptDto == null)
                throw new ArgumentNullException(nameof(acceptDto));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (!acceptDto.AgreeToTerms)
                throw new InvalidOperationException("You must agree to the terms and conditions");

            var contract = await _context.Contracts
                .Include(c => c.Booking)
                .ThenInclude(b => b != null ? b.Client : null)
                .FirstOrDefaultAsync(c => c.Id == acceptDto.ContractId);

            if (contract == null)
                throw new KeyNotFoundException("Contract not found");

            if (contract.Status != ContractStatus.PendingAcceptance)
                throw new InvalidOperationException($"Contract cannot be accepted. Current status: {contract.Status}");

            contract.Status = ContractStatus.Accepted;
            contract.AcceptedAt = DateTime.UtcNow;
            contract.AcceptedBy = acceptDto.AcceptedBy ?? userId;
            contract.AcceptedIpAddress = ipAddress;

            await _context.SaveChangesAsync();

            var result = await GetContractById(contract.Id);
            return result ?? throw new InvalidOperationException("Failed to retrieve accepted contract");
        }

        public async Task<ContractResponseDto> ActivateContract(int contractId, string adminId)
        {
            if (string.IsNullOrEmpty(adminId))
                throw new ArgumentException("Admin ID is required", nameof(adminId));

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null)
                throw new KeyNotFoundException("Contract not found");

            if (contract.Status != ContractStatus.Accepted)
                throw new InvalidOperationException("Contract must be accepted before activation");

            contract.Status = ContractStatus.Active;
            contract.ActivatedAt = DateTime.UtcNow;

            var booking = await _context.Bookings.FindAsync(contract.BookingId);
            if (booking != null)
            {
                booking.Status = BookingStatus.Active;
            }

            await _context.SaveChangesAsync();

            var result = await GetContractById(contractId);
            return result ?? throw new InvalidOperationException("Failed to retrieve activated contract");
        }

        public async Task<ContractResponseDto?> GetContractById(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return null;

            return MapToResponseDto(contract);
        }

        public async Task<IEnumerable<ContractResponseDto>> GetClientContracts(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Enumerable.Empty<ContractResponseDto>();

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null)
                return Enumerable.Empty<ContractResponseDto>();

            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .Where(c => c.ClientId == client.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<ContractResponseDto>();
            foreach (var contract in contracts)
            {
                var dto = MapToResponseDto(contract);
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        public async Task<IEnumerable<ContractResponseDto>> GetAllContracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.Client : null)
                .Include(c => c.Booking)
                    .ThenInclude(b => b != null ? b.StorageUnit : null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<ContractResponseDto>();
            foreach (var contract in contracts)
            {
                var dto = MapToResponseDto(contract);
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        public async Task<bool> ContractExistsForBooking(int bookingId)
        {
            return await _context.Contracts.AnyAsync(c => c.BookingId == bookingId);
        }

        private string GenerateContractNumber()
        {
            return $"CTR-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private string GetStandardTerms()
        {
            return @"SMARTSTORAGE STORAGE CONTRACT TERMS AND CONDITIONS

1. STORAGE PERIOD
   The storage period shall commence on the Start Date and terminate on the End Date as specified in this contract.

2. PAYMENT TERMS
   - Monthly rental fees are payable in advance on the first day of each month.
   - A security deposit equivalent to one month's rental is payable upon contract acceptance.
   - Late payments will incur a 10% penalty fee.

3. ACCESS
   - Client has 24/7 access to the storage unit using their personal access code.
   - Access is strictly for the Client and authorized persons listed in this contract.

4. PROHIBITED ITEMS
   The following items are strictly prohibited:
   - Hazardous, toxic, or flammable materials
   - Illegal substances or stolen goods
   - Perishable goods or food items
   - Living animals or plants

5. INSURANCE
   - Client is responsible for insuring all stored items.
   - SmartStorage provides basic coverage for structural damage only.

6. LIABILITY
   - SmartStorage shall not be liable for any loss or damage to stored items.
   - Client assumes all risk for stored items.

7. TERMINATION
   - Either party may terminate this contract with 30 days written notice.
   - SmartStorage reserves the right to terminate immediately for breach of terms.

8. GOVERNING LAW
   This contract is governed by the laws of South Africa.

I, the undersigned, acknowledge that I have read, understood, and agree to be bound by these terms and conditions.";
        }

        private ContractResponseDto MapToResponseDto(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var booking = contract.Booking;
            var client = booking?.Client;
            var storageUnit = booking?.StorageUnit;

            var startDate = contract.StartDate;
            var endDate = contract.EndDate;
            var durationMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month);
            if (durationMonths <= 0) durationMonths = 1;

            return new ContractResponseDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber ?? string.Empty,
                BookingId = contract.BookingId,
                BookingNumber = booking?.BookingNumber ?? string.Empty,
                ClientId = contract.ClientId,
                ClientName = client?.FullName ?? "Unknown",
                ClientEmail = client?.Email ?? "Unknown",
                UnitNumber = storageUnit?.UnitNumber ?? "Unknown",
                UnitSize = storageUnit?.Size ?? "Unknown",
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                DurationMonths = durationMonths,
                MonthlyRate = contract.MonthlyRate,
                SecurityDeposit = contract.SecurityDeposit,
                TotalContractValue = contract.TotalContractValue,
                TermsAndConditions = contract.TermsAndConditions ?? string.Empty,
                SpecialConditions = contract.SpecialConditions ?? string.Empty,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt,
                AcceptedAt = contract.AcceptedAt,
                ActivatedAt = contract.ActivatedAt,
                CanAccept = contract.Status == ContractStatus.PendingAcceptance,
                CanActivate = contract.Status == ContractStatus.Accepted
            };
        }
    }
}