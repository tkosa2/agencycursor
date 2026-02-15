using AgencyCursor.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgencyCursor.Services;

public class InvoicePdfService
{
    public byte[] GeneratePdf(Invoice invoice, bool detailedFormat = false)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        if (detailedFormat)
        {
            return GenerateDetailedPdf(invoice);
        }
        else
        {
            return GenerateStandardPdf(invoice);
        }
    }
    
    private byte[] GenerateStandardPdf(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeColumn().Column(column =>
                        {
                            column.Item().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                            column.Item().Text($"Invoice #: {invoice.InvoiceNumber ?? $"INV-{invoice.Id}"}").FontSize(12);
                            column.Item().Text($"Date: {DateTime.Now:MMMM dd, yyyy}").FontSize(12);
                        });

                        row.ConstantColumn(100).Column(column =>
                        {
                            column.Item().Text("Interpreting Agency").FontSize(12).Bold();
                            column.Item().Text("123 Business Street").FontSize(10);
                            column.Item().Text("City, State 12345").FontSize(10);
                            column.Item().Text("Phone: (555) 123-4567").FontSize(10);
                            column.Item().Text("Email: info@agency.com").FontSize(10);
                        });
                    });

                page.Content()
                    .PaddingVertical(1f, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Bill To Section
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().Text("Bill To:").FontSize(12).Bold();
                                col.Item().Text(invoice.Requestor?.Name ?? "N/A").FontSize(11);
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Address))
                                {
                                    col.Item().Text(invoice.Requestor.Address).FontSize(10);
                                }
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Phone))
                                {
                                    col.Item().Text($"Phone: {invoice.Requestor.Phone}").FontSize(10);
                                }
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Email))
                                {
                                    col.Item().Text($"Email: {invoice.Requestor.Email}").FontSize(10);
                                }
                            });

                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().Text("Service Details:").FontSize(12).Bold();
                                col.Item().Text($"Interpreter: {invoice.Interpreter?.Name ?? "N/A"}").FontSize(10);
                                col.Item().Text($"Appointment #: {invoice.AppointmentId}").FontSize(10);
                                if (invoice.Appointment?.ServiceDateTime != null)
                                {
                                    col.Item().Text($"Date: {invoice.Appointment.ServiceDateTime:MMMM dd, yyyy}").FontSize(10);
                                    col.Item().Text($"Time: {invoice.Appointment.ServiceDateTime:hh:mm tt}").FontSize(10);
                                }
                            });
                        });

                        column.Item().PaddingTop(0.5f, Unit.Centimetre);

                        // Service Type
                        if (!string.IsNullOrEmpty(invoice.ServiceType))
                        {
                            column.Item().Text($"Service Type: {invoice.ServiceType}").FontSize(11).Bold();
                            column.Item().PaddingTop(0.3f, Unit.Centimetre);
                        }

                        // Invoice Items Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Description").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Hours").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Rate").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Discount").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Amount").Bold();
                            });

                            // Content
                            var subtotal = (invoice.HoursWorked * invoice.HourlyRate) - invoice.Discount;
                            table.Cell().Element(CellStyle).Text(invoice.ServiceType ?? "Interpreting Service");
                            table.Cell().Element(CellStyle).AlignRight().Text(invoice.HoursWorked.ToString("F2"));
                            table.Cell().Element(CellStyle).AlignRight().Text(invoice.HourlyRate.ToString("C"));
                            table.Cell().Element(CellStyle).AlignRight().Text(invoice.Discount.ToString("C"));
                            table.Cell().Element(CellStyle).AlignRight().Text(invoice.TotalCost.ToString("C"));
                        });

                        column.Item().PaddingTop(0.5f, Unit.Centimetre);

                        // Totals
                        column.Item().AlignRight().Column(col =>
                        {
                            var subtotal = (invoice.HoursWorked * invoice.HourlyRate);
                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(100).Text("Subtotal:").FontSize(10);
                                row.RelativeColumn().AlignRight().Text(subtotal.ToString("C")).FontSize(10);
                            });

                            if (invoice.Discount > 0)
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantColumn(100).Text("Discount:").FontSize(10);
                                    row.RelativeColumn().AlignRight().Text($"-{invoice.Discount.ToString("C")}").FontSize(10);
                                });
                            }

                            col.Item().Row(row =>
                            {
                                row.ConstantColumn(100).Text("Total:").FontSize(12).Bold();
                                row.RelativeColumn().AlignRight().Text(invoice.TotalCost.ToString("C")).FontSize(12).Bold();
                            });
                        });

                        column.Item().PaddingTop(1f, Unit.Centimetre);

                        // Payment Status
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Text($"Payment Status: {invoice.PaymentStatus}").FontSize(11).Bold();
                            if (!string.IsNullOrEmpty(invoice.PaymentMethod))
                            {
                                row.RelativeColumn().AlignRight().Text($"Payment Method: {invoice.PaymentMethod}").FontSize(11);
                            }
                        });

                        // Notes
                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            column.Item().PaddingTop(0.5f, Unit.Centimetre);
                            column.Item().Text("Notes:").FontSize(11).Bold();
                            column.Item().Text(invoice.Notes).FontSize(10);
                        }

                        // Footer
                        column.Item().PaddingTop(1f, Unit.Centimetre);
                        column.Item().BorderTop(1f).PaddingTop(0.5f, Unit.Centimetre)
                            .Text("Thank you for your business!")
                            .FontSize(10)
                            .Italic()
                            .AlignCenter();
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
    
    private byte[] GenerateDetailedPdf(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeColumn().Column(column =>
                        {
                            column.Item().Text("Interpreting Agency").FontSize(12).Bold();
                            column.Item().Text("P.O. Box 453").FontSize(10);
                            column.Item().Text("City, State 12345").FontSize(10);
                            column.Item().PaddingTop(0.2f, Unit.Centimetre);
                            column.Item().Text("Email: billing@agency.com").FontSize(10);
                            column.Item().Text("Phone: (555) 123-4567").FontSize(10);
                            column.Item().Text("Fax: (555) 123-4568").FontSize(10);
                        });

                        row.ConstantColumn(120).Column(column =>
                        {
                            column.Item().AlignRight().Text("INVOICE").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                            column.Item().AlignRight().Text($"INVOICE #{invoice.InvoiceNumber ?? $"INV-{invoice.Id}"}").FontSize(12);
                            column.Item().AlignRight().Text($"DATE: {DateTime.Now:MMMM dd, yyyy}").FontSize(12);
                        });
                    });

                page.Content()
                    .PaddingVertical(1f, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Requestor Information
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Column(col =>
                            {
                                col.Item().Text("Requestor Name:").FontSize(11).Bold();
                                col.Item().Text(invoice.Requestor?.Name ?? "N/A").FontSize(10);
                                
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Phone))
                                {
                                    col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                    col.Item().Text("Requestor Phone:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Requestor.Phone).FontSize(10);
                                }
                                
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Email))
                                {
                                    col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                    col.Item().Text("Requestor Email:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Requestor.Email).FontSize(10);
                                }
                                
                                col.Item().PaddingTop(0.3f, Unit.Centimetre);
                                col.Item().Text("Billing Organization Name:").FontSize(11).Bold();
                                col.Item().Text(invoice.Requestor?.Name ?? "N/A").FontSize(10);
                                
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Email))
                                {
                                    col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                    col.Item().Text("Email:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Requestor.Email).FontSize(10);
                                }
                                
                                if (!string.IsNullOrEmpty(invoice.Requestor?.Phone))
                                {
                                    col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                    col.Item().Text("Billing Phone:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Requestor.Phone).FontSize(10);
                                }
                            });

                            // Appointment Details
                            row.RelativeColumn().Column(col =>
                            {
                                if (invoice.Appointment != null)
                                {
                                    col.Item().Text("Appointment Date:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Appointment.ServiceDateTime.ToString("MM/dd/yyyy")).FontSize(10);
                                    
                                    col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                    col.Item().Text("Appointment Start Time:").FontSize(11).Bold();
                                    col.Item().Text(invoice.Appointment.ServiceDateTime.ToString("h:mmtt").ToUpper()).FontSize(10);
                                    
                                    if (invoice.Appointment.DurationMinutes.HasValue)
                                    {
                                        var endTime = invoice.Appointment.ServiceDateTime.AddMinutes(invoice.Appointment.DurationMinutes.Value);
                                        col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                        col.Item().Text("Appointment End Time:").FontSize(11).Bold();
                                        col.Item().Text(endTime.ToString("h:mmtt").ToUpper()).FontSize(10);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(invoice.Appointment.ServiceDetails))
                                    {
                                        col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                        col.Item().Text("Appointment Setting:").FontSize(11).Bold();
                                        col.Item().Text(invoice.Appointment.ServiceDetails).FontSize(10);
                                    }
                                    
                                    var request = invoice.Appointment.Request;
                                    if (request != null && !string.IsNullOrEmpty(request.RequestName))
                                    {
                                        col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                        col.Item().Text("Consumer:").FontSize(11).Bold();
                                        col.Item().Text(request.RequestName).FontSize(10);
                                    }
                                    else if (invoice.Requestor != null)
                                    {
                                        col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                        col.Item().Text("Consumer:").FontSize(11).Bold();
                                        col.Item().Text(invoice.Requestor.Name).FontSize(10);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(invoice.Appointment.Location))
                                    {
                                        col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                        col.Item().Text("Location:").FontSize(11).Bold();
                                        col.Item().Text(invoice.Appointment.Location).FontSize(10);
                                    }
                                    else if (request != null)
                                    {
                                        var locationParts = new List<string>();
                                        if (!string.IsNullOrEmpty(request.Address))
                                            locationParts.Add(request.Address);
                                        if (!string.IsNullOrEmpty(request.City))
                                            locationParts.Add(request.City);
                                        if (!string.IsNullOrEmpty(request.State))
                                            locationParts.Add(request.State);
                                        if (!string.IsNullOrEmpty(request.ZipCode))
                                            locationParts.Add(request.ZipCode);
                                        
                                        if (locationParts.Any())
                                        {
                                            col.Item().PaddingTop(0.2f, Unit.Centimetre);
                                            col.Item().Text("Location:").FontSize(11).Bold();
                                            col.Item().Text(string.Join(", ", locationParts)).FontSize(10);
                                        }
                                    }
                                }
                            });
                        });

                        column.Item().PaddingTop(0.8f, Unit.Centimetre);

                        // Invoice Items Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("DESCRIPTION").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("AMOUNT").Bold();
                            });

                            // Content
                            var interpreterName = invoice.Interpreter?.Name ?? "Interpreter";
                            var rateCalculation = $"{invoice.HourlyRate:C} * {invoice.HoursWorked:F1}";
                            var lineTotal = invoice.HoursWorked * invoice.HourlyRate;
                            
                            // Interpreter name
                            table.Cell().Element(CellStyle).Text(interpreterName).FontSize(10);
                            table.Cell().Element(CellStyle);
                            
                            // Rate calculation line
                            table.Cell().Element(CellStyle).Text($"Interpreting Rate: {rateCalculation} = {lineTotal:C}").FontSize(10);
                            table.Cell().Element(CellStyle);
                            
                            // Total line
                            table.Cell().Element(CellStyle).Text("TOTAL:").FontSize(10).Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text(lineTotal.ToString("C")).FontSize(10).Bold();
                        });

                        column.Item().PaddingTop(0.5f, Unit.Centimetre);

                        // Grand Total
                        column.Item().Row(row =>
                        {
                            row.ConstantColumn(200).Text("GRAND TOTAL:").FontSize(12).Bold();
                            row.RelativeColumn().AlignRight().Text(invoice.TotalCost.ToString("C")).FontSize(12).Bold();
                        });

                        column.Item().PaddingTop(1f, Unit.Centimetre);

                        // Payment Status
                        column.Item().Row(row =>
                        {
                            row.RelativeColumn().Text($"Payment Status: {invoice.PaymentStatus}").FontSize(11).Bold();
                            if (!string.IsNullOrEmpty(invoice.PaymentMethod))
                            {
                                row.RelativeColumn().AlignRight().Text($"Payment Method: {invoice.PaymentMethod}").FontSize(11);
                            }
                        });

                        // Notes
                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            column.Item().PaddingTop(0.5f, Unit.Centimetre);
                            column.Item().Text("Notes:").FontSize(11).Bold();
                            column.Item().Text(invoice.Notes).FontSize(10);
                        }

                        // Footer
                        column.Item().PaddingTop(1f, Unit.Centimetre);
                        column.Item().BorderTop(1f).PaddingTop(0.5f, Unit.Centimetre)
                            .Text("Thank you for your business!")
                            .FontSize(10)
                            .Italic()
                            .AlignCenter();
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(0.3f, Unit.Centimetre)
            .PaddingHorizontal(0.2f, Unit.Centimetre);
    }
}
