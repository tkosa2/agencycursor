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
                .Include(a => a.Interpreter)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (Appointment == null) return NotFound();
            Invoice = new Invoice
            {
                RequestorId = Appointment.Request.RequestorId,
                AppointmentId = Appointment.Id,
                InterpreterId = Appointment.InterpreterId,
                ServiceType = Appointment.ServiceDetails ?? Appointment.Request.TypeOfService,
                HoursWorked = Appointment.DurationMinutes.HasValue ? (decimal)Appointment.DurationMinutes.Value / 60 : 1,
                HourlyRate = 0,
                Discount = 0,
                TotalCost = 0,
                PaymentStatus = "Pending",
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}"
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
                .Include(a => a.Interpreter)
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
                Invoice.InterpreterId = first.InterpreterId;
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
        var appt = await _db.Appointments.Include(a => a.Request).Include(a => a.Interpreter).FirstOrDefaultAsync(a => a.Id == Invoice.AppointmentId);
        if (appt != null)
        {
            Invoice.RequestorId = appt.Request!.RequestorId;
            Invoice.InterpreterId = appt.InterpreterId;
        }
        _db.Invoices.Add(Invoice);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
