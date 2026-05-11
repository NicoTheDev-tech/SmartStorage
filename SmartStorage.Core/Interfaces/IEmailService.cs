using SmartStorage.Core.Entities;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendContractEmailAsync(Contract contract, string clientEmail, string clientName);
        Task SendInvoiceEmailAsync(Invoice invoice, string clientEmail, string clientName);
        Task SendBookingConfirmationAsync(Booking booking, string clientEmail, string clientName);
        Task SendBookingPendingEmailAsync(Booking booking, string clientEmail, string clientName);  // ADD THIS LINE
        Task SendPaymentReceiptAsync(Payment payment, Invoice invoice, string clientEmail, string clientName);
    }
}