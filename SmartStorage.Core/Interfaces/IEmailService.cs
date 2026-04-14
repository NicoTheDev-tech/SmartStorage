using SmartStorage.Core.Entities;

namespace SmartStorage.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendContractEmailAsync(Contract contract, string clientEmail, string clientName);
        Task SendInvoiceEmailAsync(Invoice invoice, string clientEmail, string clientName);
        Task SendBookingConfirmationAsync(Booking booking, string clientEmail, string clientName);
        Task SendPaymentReceiptAsync(Payment payment, Invoice invoice, string clientEmail, string clientName);
    }
}