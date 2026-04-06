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
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(ApplicationDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InvoiceDto> ProcessPayment(ProcessPaymentDto paymentDto, string userId)
        {
            var invoice = await _context.Invoices.FindAsync(paymentDto.InvoiceId);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found");

            invoice.AmountPaid = paymentDto.Amount;
            invoice.Balance = invoice.Amount - paymentDto.Amount;
            invoice.PaymentReference = paymentDto.PaymentReference;
            invoice.PaymentMethod = paymentDto.PaymentMethod;
            invoice.PaymentDate = DateTime.UtcNow;

            if (invoice.Balance <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(invoice);
        }

        public async Task<InvoiceDto?> GetInvoiceById(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return null;
            return MapToDto(invoice);
        }

        public async Task<IEnumerable<InvoiceDto>> GetClientInvoices(string userId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (client == null) return new List<InvoiceDto>();

            var invoices = await _context.Invoices
                .Where(i => i.ClientId == client.Id)
                .ToListAsync();

            var result = new List<InvoiceDto>();
            foreach (var invoice in invoices)
            {
                result.Add(MapToDto(invoice));
            }
            return result;
        }

        private InvoiceDto MapToDto(Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                ContractId = invoice.ContractId ?? 0,
                Amount = invoice.Amount,
                AmountPaid = invoice.AmountPaid,
                Balance = invoice.Balance,
                Status = invoice.Status.ToString(),
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                PaymentReference = invoice.PaymentReference,
                PaymentDate = invoice.PaymentDate,
                PaymentMethod = invoice.PaymentMethod
            };
        }

        // These methods are not needed for basic payment flow
        public Task<InvoiceDto> GenerateInvoice(CreateInvoiceDto createDto, string userId) => throw new NotImplementedException();
        public Task<IEnumerable<InvoiceDto>> GetContractInvoices(int contractId) => throw new NotImplementedException();
        public Task<IEnumerable<InvoiceDto>> GetAllInvoices() => throw new NotImplementedException();
        public Task<InvoiceDto> VerifyPayment(PaymentConfirmationDto confirmationDto, string adminId) => throw new NotImplementedException();
        public Task<IEnumerable<InvoiceDto>> GetOverdueInvoices() => throw new NotImplementedException();
        public Task SendPaymentReminders() => throw new NotImplementedException();
    }
}