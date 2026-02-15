using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Appointment Appointment { get; set; } = null!;

    public new Request? Request { get; set; }
    public SelectList InterpreterList { get; set; } = null!;
    public SelectList? RequestList { get; set; }

    public async Task<IActionResult> OnGetAsync(int? requestId)
    {
        // Only show registered interpreters
        var interpreters = await _db.Interpreters
            .Where(i => i.IsRegisteredWithAgency)
            .OrderBy(i => i.Name)
            .ToListAsync();
        InterpreterList = new SelectList(interpreters, "Id", "Name");
        var requests = await _db.Requests.Include(r => r.Requestor).OrderByDescending(r => r.ServiceDateTime).ToListAsync();
        RequestList = new SelectList(requests.Select(r => new { r.Id, Display = $"#{r.Id} - {r.Requestor?.Name} - {r.ServiceDateTime:g}" }), "Id", "Display");
        if (requestId.HasValue)
        {
            Request = await _db.Requests.Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == requestId);
            if (Request == null) return NotFound();
            Appointment = new Appointment
            {
                RequestId = Request.Id,
                ServiceDateTime = Request.ServiceDateTime,
                Location = Request.Location ?? "",
                ServiceDetails = Request.TypeOfService,
                Status = "Pending"
            };
            if (Request.PreferredInterpreterId.HasValue)
                Appointment.InterpreterId = Request.PreferredInterpreterId.Value;
        }
        else
        {
            Appointment = new Appointment
            {
                ServiceDateTime = DateTime.Now.AddDays(1).Date.AddHours(9), // Default to tomorrow at 9 AM
                Status = "Pending"
            };
            if (requests.Count > 0)
                Appointment.RequestId = requests[0].Id;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear ModelState errors for our custom validation fields to avoid conflicts
        ModelState.Remove("Appointment.InterpreterId");
        ModelState.Remove("Appointment.RequestId");
        ModelState.Remove("Appointment.ServiceDateTime");

        // Validate required fields explicitly
        bool hasErrors = false;

        // Check InterpreterId - must be greater than 0
        if (Appointment.InterpreterId <= 0)
        {
            ModelState.AddModelError("Appointment.InterpreterId", "Please select an interpreter.");
            hasErrors = true;
        }
        else
        {
            // Verify interpreter exists and is registered
            var interpreterExists = await _db.Interpreters.AnyAsync(i => i.Id == Appointment.InterpreterId && i.IsRegisteredWithAgency);
            if (!interpreterExists)
            {
                ModelState.AddModelError("Appointment.InterpreterId", "Selected interpreter is not registered with the agency.");
                hasErrors = true;
            }
        }

        // RequestId is optional - if not provided, we'll create a request with admin requestor
        // If provided, verify it exists
        if (Appointment.RequestId > 0)
        {
            var requestExists = await _db.Requests.AnyAsync(r => r.Id == Appointment.RequestId);
            if (!requestExists)
            {
                ModelState.AddModelError("Appointment.RequestId", "Selected request does not exist.");
                hasErrors = true;
            }
        }

        // Validate ServiceDateTime - check if it's unset (default/min value or year 1)
        var minValidDate = new DateTime(1900, 1, 1);
        if (Appointment.ServiceDateTime < minValidDate)
        {
            ModelState.AddModelError("Appointment.ServiceDateTime", "Please select a date and time.");
            hasErrors = true;
        }

        if (hasErrors)
        {
            // Reload dropdowns
            await ReloadDropdownsAsync();
            return Page();
        }

        // If we get here, validation passed - proceed with save

        try
        {
            Request? request = null;
            
            // If no request is selected, create one with admin requestor
            if (Appointment.RequestId <= 0)
            {
                // Get or create admin requestor
                var adminRequestor = await _db.Requestors
                    .FirstOrDefaultAsync(r => r.Name == "Admin");
                
                if (adminRequestor == null)
                {
                    adminRequestor = new Requestor
                    {
                        Name = "Admin",
                        Email = "admin@agency.example.com",
                        Phone = "+1 (555) 000-0000",
                        Address = "Administrative Office",
                        Notes = "System requestor for admin-created appointments"
                    };
                    _db.Requestors.Add(adminRequestor);
                    await _db.SaveChangesAsync();
                }

                // Create a new request with admin requestor
                request = new Request
                {
                    RequestorId = adminRequestor.Id,
                    ServiceDateTime = Appointment.ServiceDateTime,
                    Location = Appointment.Location,
                    TypeOfService = Appointment.ServiceDetails ?? "Admin Created",
                    Status = "Assigned"
                };
                _db.Requests.Add(request);
                await _db.SaveChangesAsync();
                
                Appointment.RequestId = request.Id;
            }
            else
            {
                // Use existing request
                request = await _db.Requests.FindAsync(Appointment.RequestId);
                if (request != null)
                {
                    request.Status = "Assigned";
                }
            }

            _db.Appointments.Add(Appointment);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment created successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving appointment: {ex.Message}");
            await ReloadDropdownsAsync();
            return Page();
        }
    }

    private async Task ReloadDropdownsAsync()
    {
        var interpreters = await _db.Interpreters
            .Where(i => i.IsRegisteredWithAgency)
            .OrderBy(i => i.Name)
            .ToListAsync();
        InterpreterList = new SelectList(interpreters, "Id", "Name", Appointment.InterpreterId);
        var requests = await _db.Requests.Include(r => r.Requestor).OrderByDescending(r => r.ServiceDateTime).ToListAsync();
        RequestList = new SelectList(requests.Select(r => new { r.Id, Display = $"#{r.Id} - {r.Requestor?.Name} - {r.ServiceDateTime:g}" }), "Id", "Display");
        Request = await _db.Requests.Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == Appointment.RequestId);
    }
}
