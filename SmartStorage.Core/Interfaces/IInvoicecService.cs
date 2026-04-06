using SmartStorage.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GenerateInvoice(CreateInvoiceDto createDto, string userId);
        Task<InvoiceDto?> GetInvoiceById(int id);
        Task<IEnumerable<InvoiceDto>> GetClientInvoices(string userId);
        Task<IEnumerable<InvoiceDto>> GetContractInvoices(int contractId);
        Task<IEnumerable<InvoiceDto>> GetAllInvoices();
        Task<InvoiceDto> ProcessPayment(ProcessPaymentDto paymentDto, string userId);
        Task<InvoiceDto> VerifyPayment(PaymentConfirmationDto confirmationDto, string adminId);
        Task<IEnumerable<InvoiceDto>> GetOverdueInvoices();
        Task SendPaymentReminders();
    }
}