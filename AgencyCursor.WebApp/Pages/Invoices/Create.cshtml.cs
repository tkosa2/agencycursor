using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Invoices;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Invoice Invoice { get; set; } = null!;

    public Appointment? Appointment { get; set; }
    public SelectList? AppointmentList { get; set; }

    public async Task<IActionResult> OnGetAsync(int? appointmentId)
    {
        if (appointmentId.HasValue)
        {
            Appointment = await _db.Appointments
                .Include(a => a.Request)
                .ThenInclude(r => r!.Requestor)
                .Include(a => a.AppointmentInterpreters)
                .ThenInclude(ai => ai.Interpreter)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (Appointment == null) return NotFound();
            var durationHours = Appointment.DurationMinutes.HasValue
                ? (decimal)Appointment.DurationMinutes.Value / 60
                : 0m;
            var appointmentStatus = (Appointment.Status ?? string.Empty).Trim();
            var normalizedStatus = appointmentStatus.Replace(" ", string.Empty);
            var isCancelledWithFee = normalizedStatus.Contains("<48h", StringComparison.OrdinalIgnoreCase);
            var cancellationHours = Math.Max(2m, durationHours > 0 ? durationHours : 2m);
            // Use first interpreter from the team, or fallback to old single InterpreterId if available
            var interpreterId = Appointment.AppointmentInterpreters.FirstOrDefault()?.InterpreterId ?? Appointment.InterpreterId;
            Invoice = new Invoice
            {
                RequestorId = Appointment.Request.RequestorId,
                AppointmentId = Appointment.Id,
                InterpreterId = interpreterId,
                ServiceType = isCancelledWithFee
                    ? "Cancellation Fee"
                    : Appointment.ServiceDetails ?? Appointment.Request.TypeOfService,
                HoursWorked = isCancelledWithFee
                    ? cancellationHours
                    : (durationHours > 0 ? durationHours : 1),
                HourlyRate = 0,
                Discount = 0,
                TotalCost = 0,
                PaymentStatus = "Pending",
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                Notes = isCancelledWithFee
                    ? "Cancelled <48h - minimum 2 hours billed."
                    : null
            };
        }
        else
        {
            Invoice = new Invoice
            {
                PaymentStatus = "Pending",
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}"
            };
            var appointments = await _db.Appointments
                .Include(a => a.Request)
                .ThenInclude(r => r!.Requestor)
                .Include(a => a.AppointmentInterpreters)
                .ThenInclude(ai => ai.Interpreter)
                .OrderByDescending(a => a.ServiceDateTime)
                .ToListAsync();
            AppointmentList = new SelectList(
                appointments.Select(a => new { a.Id, Display = $"Appt #{a.Id} - {a.Request?.Requestor?.Name} - {a.ServiceDateTime:g}" }),
                "Id", "Display");
            if (appointments.Count > 0)
            {
                var first = appointments[0];
                Invoice.AppointmentId = first.Id;
                Invoice.RequestorId = first.Request!.RequestorId;
                // Use first interpreter from the team, or fallback to old single InterpreterId if available
                Invoice.InterpreterId = first.AppointmentInterpreters.FirstOrDefault()?.InterpreterId ?? first.InterpreterId;
                Invoice.ServiceType = first.ServiceDetails ?? first.Request.TypeOfService;
                Invoice.HoursWorked = first.DurationMinutes.HasValue ? (decimal)first.DurationMinutes.Value / 60 : 1;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Invoice.HourlyRate > 0 && Invoice.HoursWorked > 0)
            Invoice.TotalCost = Invoice.HoursWorked * Invoice.HourlyRate - Invoice.Discount;
        if (!ModelState.IsValid)
        {
            var appointments = await _db.Appointments
                .Include(a => a.Request).ThenInclude(r => r!.Requestor)
                .Include(a => a.Interpreter)
                .OrderByDescending(a => a.ServiceDateTime).ToListAsync();
            AppointmentList = new SelectList(
                appointments.Select(a => new { a.Id, Display = $"Appt #{a.Id} - {a.Request?.Requestor?.Name} - {a.ServiceDateTime:g}" }),
                "Id", "Display", Invoice.AppointmentId);
            return Page();
        }
        if (Invoice.TotalCost == 0 && Invoice.HourlyRate > 0 && Invoice.HoursWorked > 0)
            Invoice.TotalCost = Invoice.HoursWorked * Invoice.HourlyRate - Invoice.Discount;
        var appt = await _db.Appointments
            .Include(a => a.Request)
            .Include(a => a.AppointmentInterpreters)
            .FirstOrDefaultAsync(a => a.Id == Invoice.AppointmentId);
        if (appt != null)
        {
            Invoice.RequestorId = appt.Request!.RequestorId;
            // Use first interpreter from the team, or fallback to old single InterpreterId if available
            Invoice.InterpreterId = appt.AppointmentInterpreters.FirstOrDefault()?.InterpreterId ?? appt.InterpreterId;

            var persistedStatus = (appt.Status ?? string.Empty).Trim();
            var normalizedPersistedStatus = persistedStatus.Replace(" ", string.Empty);
            if (normalizedPersistedStatus.Contains("<48h", StringComparison.OrdinalIgnoreCase))
            {
                var durationHours = appt.DurationMinutes.HasValue
                    ? (decimal)appt.DurationMinutes.Value / 60
                    : 0m;
                var cancellationHours = Math.Max(2m, durationHours > 0 ? durationHours : 2m);

                if (Invoice.HoursWorked < cancellationHours)
                {
                    Invoice.HoursWorked = cancellationHours;
                }

                if (string.IsNullOrWhiteSpace(Invoice.ServiceType))
                {
                    Invoice.ServiceType = "Cancellation Fee";
                }

                if (string.IsNullOrWhiteSpace(Invoice.Notes))
                {
                    Invoice.Notes = "Cancelled <48h - minimum 2 hours billed.";
                }
            }
        }
        _db.Invoices.Add(Invoice);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
