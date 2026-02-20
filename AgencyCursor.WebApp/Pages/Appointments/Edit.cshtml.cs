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

    [BindProperty]
    public List<int> SelectedInterpreterIds { get; set; } = new();

    public SelectList InterpreterList { get; set; } = null!;
    public IList<InterpreterEmailLog> EmailLogs { get; set; } = new List<InterpreterEmailLog>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var a = await _db.Appointments
            .Include(app => app.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(app => app.AppointmentInterpreters)
            .FirstOrDefaultAsync(app => app.Id == id);
        if (a == null) return NotFound();
        Appointment = a;
        SelectedInterpreterIds = a.AppointmentInterpreters.Select(ai => ai.InterpreterId).ToList();
        InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name");
        
        // Load email logs for this appointment's request
        EmailLogs = await _db.InterpreterEmailLogs
            .Where(el => el.RequestId == Appointment.RequestId)
            .Include(el => el.Interpreter)
            .OrderByDescending(el => el.SentAt)
            .ToListAsync();
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove validation errors for navigation properties that aren't being edited
        ModelState.Remove("Appointment.Request");
        ModelState.Remove("Appointment.Interpreter");
        ModelState.Remove("Appointment.AppointmentInterpreters");
        
        // Load the existing appointment to preserve values that aren't being changed
        var existingAppointment = await _db.Appointments
            .Include(a => a.Request)
            .Include(a => a.AppointmentInterpreters)
            .FirstOrDefaultAsync(a => a.Id == Appointment.Id);
        
        if (existingAppointment == null) return NotFound();
        
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
            
            // Reload requestor info and interpreters for display
            var appointment = await _db.Appointments
                .Include(app => app.Request)
                .ThenInclude(r => r!.Requestor)
                .Include(app => app.AppointmentInterpreters)
                .FirstOrDefaultAsync(app => app.Id == Appointment.Id);
            if (appointment != null)
            {
                Appointment.Request = appointment.Request;
                SelectedInterpreterIds = appointment.AppointmentInterpreters.Select(ai => ai.InterpreterId).ToList();
            }
            InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name");
            
            // Load email logs for this appointment's request
            EmailLogs = await _db.InterpreterEmailLogs
                .Where(el => el.RequestId == Appointment.RequestId)
                .Include(el => el.Interpreter)
                .OrderByDescending(el => el.SentAt)
                .ToListAsync();
            
            return Page();
        }
        
        // Update appointment properties
        existingAppointment.ServiceDateTime = Appointment.ServiceDateTime;
        existingAppointment.Location = Appointment.Location;
        existingAppointment.ServiceDetails = Appointment.ServiceDetails;
        existingAppointment.DurationMinutes = Appointment.DurationMinutes;
        existingAppointment.Status = Appointment.Status;
        existingAppointment.AdditionalNotes = Appointment.AdditionalNotes;
        
        // Update interpreter team
        // Remove existing interpreters
        _db.AppointmentInterpreters.RemoveRange(existingAppointment.AppointmentInterpreters);
        
        // Add selected interpreters
        if (SelectedInterpreterIds != null && SelectedInterpreterIds.Any())
        {
            foreach (var interpreterId in SelectedInterpreterIds)
            {
                existingAppointment.AppointmentInterpreters.Add(new AppointmentInterpreter
                {
                    AppointmentId = existingAppointment.Id,
                    InterpreterId = interpreterId
                });
            }
        }
        
        // If appointment is cancelled, update the related request status to "Cancelled"
        if (Appointment.Status.StartsWith("Cancelled") && existingAppointment.Request != null)
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
