using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmartStorage.Core.Config;
using SmartStorage.Core.Entities;
using SmartStorage.Core.Interfaces;
using SmartStorage.Infrastructure.Data;

namespace SmartStorage.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly PdfGeneratorService _pdfGenerator;
        private readonly ILogger<EmailService> _logger;
        private readonly ApplicationDbContext _context;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            PdfGeneratorService pdfGenerator,
            ILogger<EmailService> logger,
            ApplicationDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _pdfGenerator = pdfGenerator;
            _logger = logger;
            _context = context;
        }

        public async Task SendContractEmailAsync(Contract contract, string clientEmail, string clientName)
        {
            try
            {
                if (contract == null) throw new ArgumentNullException(nameof(contract));
                if (string.IsNullOrEmpty(clientEmail)) throw new ArgumentException("Client email is required");

                // Get related data for PDF
                var booking = await _context.Bookings
                    .Include(b => b.StorageUnit)
                    .FirstOrDefaultAsync(b => b.Id == contract.BookingId);

                var unit = booking?.StorageUnit;
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == contract.ClientId);

                // Create safe copies with null checks
                var safeBooking = booking ?? new Booking();
                var safeUnit = unit ?? new StorageUnit();
                var safeClient = client ?? new Client();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(clientName, clientEmail));
                message.Subject = $"Your SmartStorage Contract - {contract.ContractNumber}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateContractEmailHtml(contract, clientName);

                var pdfBytes = _pdfGenerator.GenerateContractPdf(contract, safeBooking, safeUnit, safeClient);
                bodyBuilder.Attachments.Add($"Contract_{contract.ContractNumber}.pdf", pdfBytes);

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Contract email sent to {Email} for contract {ContractNumber}",
                    clientEmail, contract.ContractNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contract email to {Email}", clientEmail);
                throw;
            }
        }

        public async Task SendInvoiceEmailAsync(Invoice invoice, string clientEmail, string clientName)
        {
            try
            {
                if (invoice == null) throw new ArgumentNullException(nameof(invoice));
                if (string.IsNullOrEmpty(clientEmail)) throw new ArgumentException("Client email is required");

                var booking = await _context.Bookings
                    .Include(b => b.StorageUnit)
                    .FirstOrDefaultAsync(b => b.Id == invoice.BookingId);
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId);

                // Create safe copies with null checks
                var safeBooking = booking ?? new Booking();
                var safeClient = client ?? new Client();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(clientName, clientEmail));
                message.Subject = $"SmartStorage Invoice - {invoice.InvoiceNumber}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateInvoiceEmailHtml(invoice, clientName);

                var pdfBytes = _pdfGenerator.GenerateInvoicePdf(invoice, safeBooking, safeClient);
                bodyBuilder.Attachments.Add($"Invoice_{invoice.InvoiceNumber}.pdf", pdfBytes);

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Invoice email sent to {Email} for invoice {InvoiceNumber}",
                    clientEmail, invoice.InvoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice email to {Email}", clientEmail);
                throw;
            }
        }

        public async Task SendBookingConfirmationAsync(Booking booking, string clientEmail, string clientName)
        {
            try
            {
                if (booking == null) throw new ArgumentNullException(nameof(booking));
                if (string.IsNullOrEmpty(clientEmail)) throw new ArgumentException("Client email is required");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(clientName, clientEmail));
                message.Subject = $"Booking Confirmation - {booking.BookingNumber}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateBookingConfirmationHtml(booking, clientName);

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Booking confirmation sent to {Email} for booking {BookingNumber}",
                    clientEmail, booking.BookingNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation to {Email}", clientEmail);
                throw;
            }
        }

        public async Task SendPaymentReceiptAsync(Payment payment, Invoice invoice, string clientEmail, string clientName)
        {
            try
            {
                if (payment == null) throw new ArgumentNullException(nameof(payment));
                if (string.IsNullOrEmpty(clientEmail)) throw new ArgumentException("Client email is required");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(clientName, clientEmail));
                message.Subject = $"Payment Receipt - {payment.TransactionId ?? "Payment Confirmed"}";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GeneratePaymentReceiptHtml(payment, invoice, clientName);

                message.Body = bodyBuilder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    _emailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Payment receipt sent to {Email} for amount {Amount}",
                    clientEmail, payment.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment receipt to {Email}", clientEmail);
                throw;
            }
        }

        #region HTML Templates

        private string GenerateContractEmailHtml(Contract contract, string clientName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {{ font-family: Arial, sans-serif; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
    .content {{ padding: 20px; }}
    .footer {{ margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h2>SmartStorage Contract</h2>
    </div>
    <div class='content'>
        <p>Dear {clientName},</p>
        <p>Your storage contract has been created and is attached as a PDF to this email.</p>
        <p><strong>Contract Details:</strong></p>
        <ul>
            <li>Contract Number: <strong>{contract.ContractNumber}</strong></li>
            <li>Start Date: {contract.StartDate:dd MMMM yyyy}</li>
            <li>End Date: {contract.EndDate:dd MMMM yyyy}</li>
            <li>Monthly Rate: <strong>R{contract.MonthlyRate:N2}</strong></li>
            <li>Total Value: <strong>R{contract.TotalContractValue:N2}</strong></li>
        </ul>
        <p>Please review the attached contract and sign it at your earliest convenience.</p>
        <p>If you have any questions, please don't hesitate to contact us.</p>
        <p>Best regards,<br>The SmartStorage Team</p>
    </div>
    <div class='footer'>
        <p>SmartStorage | 123 Storage Street, Johannesburg | Tel: 0800 123 456</p>
    </div>
</div>
</body>
</html>";
        }

        private string GenerateInvoiceEmailHtml(Invoice invoice, string clientName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {{ font-family: Arial, sans-serif; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background-color: #2c3e50; color: white; padding: 20px; text-align: center; }}
    .content {{ padding: 20px; }}
    .amount {{ font-size: 24px; color: #27ae60; font-weight: bold; }}
    .due {{ color: #e74c3c; font-weight: bold; }}
    .footer {{ margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h2>Invoice #{invoice.InvoiceNumber}</h2>
    </div>
    <div class='content'>
        <p>Dear {clientName},</p>
        <p>Please find attached your invoice for storage services.</p>
        <p><strong>Invoice Summary:</strong></p>
        <ul>
            <li>Invoice Number: <strong>{invoice.InvoiceNumber}</strong></li>
            <li>Invoice Date: {invoice.InvoiceDate:dd MMMM yyyy}</li>
            <li>Due Date: <span class='due'>{invoice.DueDate:dd MMMM yyyy}</span></li>
            <li>Amount Due: <span class='amount'>R{invoice.Balance:N2}</span></li>
        </ul>
        <p>Please make payment by the due date to avoid late fees.</p>
        <p><strong>Payment Details:</strong></p>
        <ul>
            <li>Bank: First National Bank (FNB)</li>
            <li>Account: SmartStorage (Pty) Ltd</li>
            <li>Account Number: 1234567890</li>
            <li>Reference: {invoice.InvoiceNumber}</li>
        </ul>
        <p>Best regards,<br>The SmartStorage Team</p>
    </div>
    <div class='footer'>
        <p>SmartStorage | 123 Storage Street, Johannesburg | Tel: 0800 123 456</p>
    </div>
</div>
</body>
</html>";
        }

        private string GenerateBookingConfirmationHtml(Booking booking, string clientName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {{ font-family: Arial, sans-serif; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background-color: #27ae60; color: white; padding: 20px; text-align: center; }}
    .content {{ padding: 20px; }}
    .footer {{ margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h2>Booking Confirmed!</h2>
    </div>
    <div class='content'>
        <p>Dear {clientName},</p>
        <p>Your storage booking has been confirmed!</p>
        <p><strong>Booking Details:</strong></p>
        <ul>
            <li>Booking Number: <strong>{booking.BookingNumber}</strong></li>
            <li>Start Date: {booking.StartDate:dd MMMM yyyy}</li>
            <li>End Date: {booking.EndDate:dd MMMM yyyy}</li>
            <li>Total Amount: <strong>R{booking.TotalAmount:N2}</strong></li>
        </ul>
        <p>You will receive your contract and invoice shortly.</p>
        <p>Thank you for choosing SmartStorage!</p>
        <p>Best regards,<br>The SmartStorage Team</p>
    </div>
    <div class='footer'>
        <p>SmartStorage | 123 Storage Street, Johannesburg | Tel: 0800 123 456</p>
    </div>
</div>
</body>
</html>";
        }

        private string GeneratePaymentReceiptHtml(Payment payment, Invoice invoice, string clientName)
        {
            string paymentMethod = "Card";
            if (!string.IsNullOrEmpty(payment.PaymentReference))
            {
                paymentMethod = "EFT";
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {{ font-family: Arial, sans-serif; }}
    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background-color: #27ae60; color: white; padding: 20px; text-align: center; }}
    .content {{ padding: 20px; }}
    .amount {{ font-size: 24px; color: #27ae60; font-weight: bold; }}
    .footer {{ margin-top: 20px; padding-top: 10px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
</style>
</head>
<body>
<div class='container'>
    <div class='header'>
        <h2>Payment Receipt</h2>
    </div>
    <div class='content'>
        <p>Dear {clientName},</p>
        <p>Thank you for your payment!</p>
        <p><strong>Payment Details:</strong></p>
        <ul>
            <li>Payment Amount: <span class='amount'>R{payment.Amount:N2}</span></li>
            <li>Payment Method: {paymentMethod}</li>
            <li>Payment Date: {payment.PaymentDate:dd MMMM yyyy HH:mm}</li>
            <li>Transaction ID: {payment.TransactionId ?? "N/A"}</li>
            <li>Invoice Number: {invoice?.InvoiceNumber ?? "N/A"}</li>
        </ul>
        <p>Your payment has been processed successfully.</p>
        <p>Best regards,<br>The SmartStorage Team</p>
    </div>
    <div class='footer'>
        <p>SmartStorage | 123 Storage Street, Johannesburg | Tel: 0800 123 456</p>
    </div>
</div>
</body>
</html>";
        }

        #endregion
    }
}