using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DetailsModel(AgencyDbContext db) => _db = db;

    public Appointment? Appointment { get; set; }
    public SelectList InterpreterList { get; set; } = null!;

    [BindProperty]
    public int? SelectedInterpreterId { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        
        await LoadAppointmentDataAsync(id.Value);
        
        if (Appointment == null) return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAssignInterpreterAsync(int? id)
    {
        if (id == null) return NotFound();

        if (!SelectedInterpreterId.HasValue)
        {
            TempData["ErrorMessage"] = "Please select an interpreter.";
            // Reload data for the page
            await LoadAppointmentDataAsync(id.Value);
            return Page();
        }

        var appointment = await _db.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        var interpreter = await _db.Interpreters.FindAsync(SelectedInterpreterId.Value);
        if (interpreter == null)
        {
            TempData["ErrorMessage"] = "Selected interpreter not found.";
            // Reload data for the page
            await LoadAppointmentDataAsync(id.Value);
            return Page();
        }

        appointment.InterpreterId = SelectedInterpreterId.Value;
        
        // Update status to "Assigned" or "Confirmed" if it was Pending
        if (appointment.Status == "Pending")
        {
            appointment.Status = "Assigned";
        }

        // Update the related request status to "Confirmed" when interpreter is assigned
        var request = await _db.Requests.FindAsync(appointment.RequestId);
        if (request != null)
        {
            if (request.Status == "Pending" || request.Status == "Assigned")
            {
                request.Status = "Confirmed";
            }
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Interpreter {interpreter.Name} has been assigned to this appointment.";
        return RedirectToPage(new { id });
    }

    private async Task LoadAppointmentDataAsync(int id)
    {
        Appointment = await _db.Appointments
            .Include(a => a.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(a => a.Interpreter)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (Appointment != null)
        {
            // Load registered interpreters for assignment
            var interpreters = await _db.Interpreters
                .Where(i => i.IsRegisteredWithAgency)
                .OrderBy(i => i.Name)
                .ToListAsync();
            
            // Set SelectedInterpreterId for binding
            SelectedInterpreterId = Appointment.InterpreterId;
            // Create SelectList with the int value (not nullable) for proper selection
            InterpreterList = new SelectList(interpreters, "Id", "Name", Appointment.InterpreterId);
        }
    }
}
