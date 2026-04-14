using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartStorage.Core.Entities;

namespace SmartStorage.Infrastructure.Services
{
    public class PdfGeneratorService
    {
        public PdfGeneratorService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateContractPdf(Contract contract, Booking booking, StorageUnit unit, Client client)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .AlignCenter()
                        .Text("SMARTSTORAGE CONTRACT")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .Column(column =>
                        {
                            // Contract Info
                            column.Item().Text($"Contract Number: {contract.ContractNumber}").Bold();
                            column.Item().Text($"Date: {DateTime.Now:dd MMMM yyyy}");
                            column.Item().LineHorizontal(0.5f);
                            column.Item().PaddingBottom(10);

                            // Parties
                            column.Item().Text("1. PARTIES").Underline().Bold();
                            column.Item().Text($"Client: {client?.FullName ?? "N/A"}");
                            column.Item().Text($"Email: {client?.Email ?? "N/A"}");
                            column.Item().Text($"Phone: {client?.Phone ?? "N/A"}");
                            column.Item().Text($"Address: {client?.Address ?? "N/A"}");
                            column.Item().PaddingBottom(10);

                            // Storage Unit
                            column.Item().Text("2. STORAGE UNIT").Underline().Bold();
                            column.Item().Text($"Unit Number: {unit?.UnitNumber ?? "N/A"}");
                            column.Item().Text($"Size: {unit?.Size ?? "N/A"}");
                            column.Item().Text($"Location: {unit?.Location ?? "N/A"}");
                            column.Item().Text($"Climate Control: {unit?.ClimateControl ?? "N/A"}");
                            column.Item().PaddingBottom(10);

                            // Contract Period
                            column.Item().Text("3. CONTRACT PERIOD").Underline().Bold();
                            column.Item().Text($"Start Date: {contract.StartDate:dd MMMM yyyy}");
                            column.Item().Text($"End Date: {contract.EndDate:dd MMMM yyyy}");
                            column.Item().PaddingBottom(10);

                            // Payment Terms
                            column.Item().Text("4. PAYMENT TERMS").Underline().Bold();
                            column.Item().Text($"Monthly Rental: R{contract.MonthlyRate:N2}");
                            column.Item().Text($"Security Deposit: R{contract.SecurityDeposit:N2}");
                            column.Item().Text($"Total Contract Value: R{contract.TotalContractValue:N2}");
                            column.Item().PaddingBottom(10);

                            // Terms and Conditions
                            column.Item().Text("5. TERMS AND CONDITIONS").Underline().Bold();
                            column.Item().Text(contract.TermsAndConditions ?? "Standard terms apply.");
                            column.Item().PaddingBottom(10);

                            // Signatures
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(signature =>
                                {
                                    signature.Item().Text("Client Signature:");
                                    signature.Item().PaddingTop(20).LineHorizontal(1);
                                    signature.Item().Text(client?.FullName ?? "Client");
                                    signature.Item().Text($"Date: {DateTime.Now:dd MMMM yyyy}");
                                });

                                row.RelativeItem().Column(signature =>
                                {
                                    signature.Item().Text("SmartStorage Representative:");
                                    signature.Item().PaddingTop(20).LineHorizontal(1);
                                    signature.Item().Text("Authorized Signatory");
                                    signature.Item().Text($"Date: {DateTime.Now:dd MMMM yyyy}");
                                });
                            });
                        });

                    // FIXED: Correct footer syntax for QuestPDF
                    page.Footer()
                        .AlignCenter()
                        .Text("SmartStorage - Tel: 0800 123 456 | Email: info@smartstorage.co.za")
                        .FontSize(9);
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateInvoicePdf(Invoice invoice, Booking booking, Client client)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(header =>
                            {
                                header.Item().Text("SMARTSTORAGE")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);
                                header.Item().Text("Storage Solutions");
                            });

                            row.RelativeItem().Column(header =>
                            {
                                header.Item().AlignRight().Text("INVOICE")
                                    .FontSize(20)
                                    .Bold();
                                header.Item().AlignRight().Text($"#{invoice.InvoiceNumber}");
                            });
                        });

                    page.Content()
                        .Column(column =>
                        {
                            // Invoice Details
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(details =>
                                {
                                    details.Item().Text("BILL TO:").Bold();
                                    details.Item().Text(client?.FullName ?? "N/A");
                                    details.Item().Text(client?.Email ?? "N/A");
                                    details.Item().Text(client?.Phone ?? "N/A");
                                    details.Item().Text(client?.Address ?? "N/A");
                                });

                                row.RelativeItem().Column(details =>
                                {
                                    details.Item().AlignRight().Text("INVOICE DETAILS:").Bold();
                                    details.Item().AlignRight().Text($"Date: {invoice.InvoiceDate:dd MMMM yyyy}");
                                    details.Item().AlignRight().Text($"Due Date: {invoice.DueDate:dd MMMM yyyy}");
                                    details.Item().AlignRight().Text($"Booking: {booking?.BookingNumber ?? "N/A"}");
                                });
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(0.5f);

                            // Items Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Description").Bold();
                                    header.Cell().AlignRight().Text("Quantity").Bold();
                                    header.Cell().AlignRight().Text("Unit Price").Bold();
                                    header.Cell().AlignRight().Text("Total").Bold();
                                });

                                decimal calculatedBaseAmount = invoice.Amount - 75;

                                // Storage row
                                table.Cell().Text($"Storage Unit - {invoice.PeriodStart:dd MMM} to {invoice.PeriodEnd:dd MMM yyyy}");
                                table.Cell().AlignRight().Text("1");
                                table.Cell().AlignRight().Text($"R{calculatedBaseAmount:N2}");
                                table.Cell().AlignRight().Text($"R{calculatedBaseAmount:N2}");

                                // Admin Fee row
                                table.Cell().Text("Admin Fee");
                                table.Cell().AlignRight().Text("1");
                                table.Cell().AlignRight().Text("R25.00");
                                table.Cell().AlignRight().Text("R25.00");

                                // Security Fee row
                                table.Cell().Text("Security Deposit");
                                table.Cell().AlignRight().Text("1");
                                table.Cell().AlignRight().Text("R50.00");
                                table.Cell().AlignRight().Text("R50.00");
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(0.5f);

                            // Totals
                            column.Item().AlignRight().Column(totals =>
                            {
                                decimal calculatedBaseAmount = invoice.Amount - 75;
                                totals.Item().Text($"Subtotal: R{calculatedBaseAmount:N2}");
                                totals.Item().Text($"Admin Fee: R25.00");
                                totals.Item().Text($"Security Deposit: R50.00");
                                totals.Item().PaddingTop(5).Text($"TOTAL: R{invoice.Amount:N2}").Bold();
                            });

                            // Payment Instructions
                            column.Item().PaddingTop(20).Text("PAYMENT INSTRUCTIONS").Underline().Bold();
                            column.Item().Text("Bank: First National Bank (FNB)");
                            column.Item().Text("Account Name: SmartStorage (Pty) Ltd");
                            column.Item().Text("Account Number: 1234567890");
                            column.Item().Text("Branch Code: 250655");
                            column.Item().Text("Reference: Use your Invoice Number as reference");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for choosing SmartStorage!")
                        .FontSize(9);
                });
            });

            return document.GeneratePdf();
        }
    }
}