using SmartStorage.Core.DTOs;
using SmartStorage.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStorage.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPayment(CreatePaymentDto paymentDto);
        Task<InvoiceDto> GenerateInvoice(int bookingId);
        Task<IEnumerable<Payment>> GetBookingPayments(int bookingId);
        Task<bool> VerifyPayment(string paymentReference);
    }
}
