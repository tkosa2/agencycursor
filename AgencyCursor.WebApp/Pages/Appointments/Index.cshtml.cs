using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db) => _db = db;

    public IList<Appointment> Appointments { get; set; } = new List<Appointment>();

    public void OnGet()
    {
        Appointments = _db.Appointments
            .Include(a => a.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(a => a.AppointmentInterpreters)
            .ThenInclude(ai => ai.Interpreter)
            .OrderByDescending(a => a.ServiceDateTime)
            .ToList();
    }

    public async Task<IActionResult> OnPostCloneAsync(int? id)
    {
        if (id == null)
        {
            TempData["ErrorMessage"] = "Appointment ID is required.";
            return RedirectToPage();
        }

        var originalAppointment = await _db.Appointments
            .Include(a => a.AppointmentInterpreters)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (originalAppointment == null)
        {
            TempData["ErrorMessage"] = "Appointment not found.";
            return RedirectToPage();
        }

        // Create a clone of the appointment
        var clonedAppointment = new Appointment
        {
            RequestId = originalAppointment.RequestId,
            InterpreterId = originalAppointment.InterpreterId, // Keep for backwards compatibility
            ServiceDateTime = originalAppointment.ServiceDateTime,
            Location = originalAppointment.Location,
            Status = "Pending", // Reset status to Pending for cloned appointment
            ServiceDetails = originalAppointment.ServiceDetails,
            DurationMinutes = originalAppointment.DurationMinutes,
            AdditionalNotes = originalAppointment.AdditionalNotes != null 
                ? $"Cloned from Appointment #{originalAppointment.Id}. {originalAppointment.AdditionalNotes}"
                : $"Cloned from Appointment #{originalAppointment.Id}."
        };

        _db.Appointments.Add(clonedAppointment);
        await _db.SaveChangesAsync();

        // Clone the interpreter team
        foreach (var ai in originalAppointment.AppointmentInterpreters)
        {
            _db.AppointmentInterpreters.Add(new AppointmentInterpreter
            {
                AppointmentId = clonedAppointment.Id,
                InterpreterId = ai.InterpreterId
            });
        }
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Appointment #{originalAppointment.Id} has been cloned. New appointment ID: #{clonedAppointment.Id}";
        return RedirectToPage();
    }
}
