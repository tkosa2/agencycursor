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
    public List<MatchedInterpreter> MatchedInterpreters { get; set; } = new List<MatchedInterpreter>();
    public IList<InterpreterEmailLog> EmailLogs { get; set; } = new List<InterpreterEmailLog>();
    public Dictionary<int, InterpreterResponse?> ResponsesByInterpreterId { get; set; } = new Dictionary<int, InterpreterResponse?>();

    [BindProperty]
    public List<int> SelectedInterpreterIds { get; set; } = new List<int>();

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

        if (SelectedInterpreterIds == null || !SelectedInterpreterIds.Any())
        {
            TempData["ErrorMessage"] = "Please select at least one interpreter.";
            // Reload data for the page
            await LoadAppointmentDataAsync(id.Value);
            return Page();
        }

        var appointment = await _db.Appointments
            .Include(a => a.AppointmentInterpreters)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (appointment == null) return NotFound();

        var interpreters = await _db.Interpreters
            .Where(i => SelectedInterpreterIds.Contains(i.Id))
            .ToListAsync();
        
        if (!interpreters.Any())
        {
            TempData["ErrorMessage"] = "Selected interpreters not found.";
            // Reload data for the page
            await LoadAppointmentDataAsync(id.Value);
            return Page();
        }

        // Clear existing assignments
        _db.AppointmentInterpreters.RemoveRange(appointment.AppointmentInterpreters);
        
        // Add new assignments
        foreach (var interpreterId in SelectedInterpreterIds)
        {
            appointment.AppointmentInterpreters.Add(new AppointmentInterpreter
            {
                AppointmentId = appointment.Id,
                InterpreterId = interpreterId
            });
        }

        // Set the primary interpreter (first selected)
        appointment.InterpreterId = SelectedInterpreterIds.First();
        
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

        var interpreterNames = string.Join(", ", interpreters.Select(i => i.Name));
        TempData["SuccessMessage"] = $"Interpreter(s) {interpreterNames} have been assigned to this appointment.";
        return RedirectToPage(new { id });
    }

    private async Task LoadAppointmentDataAsync(int id)
    {
        Appointment = await _db.Appointments
            .Include(a => a.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(a => a.AppointmentInterpreters)
            .ThenInclude(ai => ai.Interpreter)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (Appointment != null)
        {
            // Load email logs for this appointment's request
            EmailLogs = await _db.InterpreterEmailLogs
                .Where(el => el.RequestId == Appointment.RequestId)
                .Include(el => el.Interpreter)
                .OrderByDescending(el => el.SentAt)
                .ToListAsync();

            // Load interpreter responses for this request
            var responses = await _db.InterpreterResponses
                .Where(ir => ir.RequestId == Appointment.RequestId)
                .Include(ir => ir.Interpreter)
                .ToListAsync();

            ResponsesByInterpreterId = responses
                .GroupBy(ir => ir.InterpreterId)
                .ToDictionary(g => g.Key, g => (InterpreterResponse?)g.FirstOrDefault());

            // Build list of matched interpreters who were contacted
            var contactedInterpreterIds = EmailLogs.Select(el => el.InterpreterId).Distinct().ToList();
            var interpreters = await _db.Interpreters
                .Where(i => contactedInterpreterIds.Contains(i.Id))
                .ToListAsync();

            MatchedInterpreters = interpreters.Select(i => new MatchedInterpreter
            {
                Interpreter = i,
                Response = ResponsesByInterpreterId.ContainsKey(i.Id) ? ResponsesByInterpreterId[i.Id] : null,
                IsAssigned = Appointment.AppointmentInterpreters.Any(ai => ai.InterpreterId == i.Id)
            })
            .OrderByDescending(mi => mi.Response?.Status == "Yes")
            .ThenByDescending(mi => mi.Response?.Status == "Maybe")
            .ThenBy(mi => mi.Interpreter.Name)
            .ToList();

            // Pre-select currently assigned interpreters
            SelectedInterpreterIds = Appointment.AppointmentInterpreters
                .Select(ai => ai.InterpreterId)
                .ToList();
        }
    }

    public class MatchedInterpreter
    {
        public Interpreter Interpreter { get; set; } = null!;
        public InterpreterResponse? Response { get; set; }
        public bool IsAssigned { get; set; }
    }
}
