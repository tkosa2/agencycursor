using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;

    public EditModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Appointment Appointment { get; set; } = null!;

    public SelectList InterpreterList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var a = await _db.Appointments
            .Include(app => app.Request)
            .ThenInclude(r => r!.Requestor)
            .FirstOrDefaultAsync(app => app.Id == id);
        if (a == null) return NotFound();
        Appointment = a;
        InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name", Appointment.InterpreterId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove validation errors for navigation properties that aren't being edited
        ModelState.Remove("Appointment.Request");
        ModelState.Remove("Appointment.Interpreter");
        
        // Load the existing appointment to preserve values that aren't being changed
        var existingAppointment = await _db.Appointments
            .Include(a => a.Request)
            .FirstOrDefaultAsync(a => a.Id == Appointment.Id);
        
        if (existingAppointment == null) return NotFound();
        
        // If InterpreterId is 0 or not provided, use the existing value
        if (Appointment.InterpreterId == 0)
        {
            Appointment.InterpreterId = existingAppointment.InterpreterId;
            ModelState.Remove("Appointment.InterpreterId");
        }
        
        // Ensure RequestId is preserved
        if (Appointment.RequestId == 0)
        {
            Appointment.RequestId = existingAppointment.RequestId;
            ModelState.Remove("Appointment.RequestId");
        }
        
        // If ServiceDateTime is default (not provided), use the existing value
        if (Appointment.ServiceDateTime == default(DateTime))
        {
            Appointment.ServiceDateTime = existingAppointment.ServiceDateTime;
            ModelState.Remove("Appointment.ServiceDateTime");
        }
        
        if (!ModelState.IsValid)
        {
            // Add a general error message if validation fails
            if (ModelState.ErrorCount > 0)
            {
                TempData["ErrorMessage"] = "Please correct the errors below and try again.";
            }
            
            // Reload requestor info for display
            var appointment = await _db.Appointments
                .Include(app => app.Request)
                .ThenInclude(r => r!.Requestor)
                .FirstOrDefaultAsync(app => app.Id == Appointment.Id);
            if (appointment != null)
            {
                Appointment.Request = appointment.Request;
            }
            InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name", Appointment.InterpreterId);
            return Page();
        }
        
        // existingAppointment is already loaded above, reuse it
        
        // Update appointment properties
        existingAppointment.InterpreterId = Appointment.InterpreterId;
        existingAppointment.ServiceDateTime = Appointment.ServiceDateTime;
        existingAppointment.Location = Appointment.Location;
        existingAppointment.ServiceDetails = Appointment.ServiceDetails;
        existingAppointment.DurationMinutes = Appointment.DurationMinutes;
        existingAppointment.ClientEmployeeName = Appointment.ClientEmployeeName;
        existingAppointment.Status = Appointment.Status;
        existingAppointment.AdditionalNotes = Appointment.AdditionalNotes;
        
        // If appointment is cancelled, update the related request status to "Cancelled"
        if (Appointment.Status == "Cancelled" && existingAppointment.Request != null)
        {
            existingAppointment.Request.Status = "Cancelled";
            // Explicitly mark the Request entity as modified to ensure the change is saved
            _db.Entry(existingAppointment.Request).Property(r => r.Status).IsModified = true;
        }
        
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Appointments.AnyAsync(x => x.Id == Appointment.Id)) return NotFound();
            throw;
        }
        return RedirectToPage("Index");
    }
}
