using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Interpreters;

public class RespondToRequestModel : PageModel
{
    private readonly AgencyDbContext _db;

    public RespondToRequestModel(AgencyDbContext db) => _db = db;

    public Request? Request { get; set; }
    public Interpreter? CurrentInterpreter { get; set; }
    public InterpreterResponse? CurrentResponse { get; set; }

    [BindProperty]
    public string ResponseStatus { get; set; } = string.Empty; // "Yes", "No", "Maybe"

    [BindProperty]
    public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync(int requestId, int interpreterId)
    {
        // Load request and interpreter
        Request = await _db.Requests
            .Include(r => r.Requestor)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (Request == null)
            return NotFound();

        CurrentInterpreter = await _db.Interpreters.FirstOrDefaultAsync(i => i.Id == interpreterId);
        if (CurrentInterpreter == null)
            return NotFound();

        // Check if they've already responded
        CurrentResponse = await _db.InterpreterResponses
            .FirstOrDefaultAsync(ir => ir.RequestId == requestId && ir.InterpreterId == interpreterId);

        // Pre-populate with previous response if it exists
        if (CurrentResponse != null)
        {
            ResponseStatus = CurrentResponse.Status;
            Notes = CurrentResponse.Notes;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int requestId, int interpreterId)
    {
        Request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (Request == null)
            return NotFound();

        CurrentInterpreter = await _db.Interpreters.FirstOrDefaultAsync(i => i.Id == interpreterId);
        if (CurrentInterpreter == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(ResponseStatus) || !new[] { "Yes", "No", "Maybe" }.Contains(ResponseStatus))
        {
            ModelState.AddModelError("ResponseStatus", "Please select a response option.");
            return Page();
        }

        // Check if response already exists
        var existingResponse = await _db.InterpreterResponses
            .FirstOrDefaultAsync(ir => ir.RequestId == requestId && ir.InterpreterId == interpreterId);

        if (existingResponse != null)
        {
            // Update existing response
            existingResponse.Status = ResponseStatus;
            existingResponse.Notes = Notes;
            existingResponse.RespondedAt = DateTime.UtcNow;
            _db.InterpreterResponses.Update(existingResponse);
        }
        else
        {
            // Create new response
            var response = new InterpreterResponse
            {
                RequestId = requestId,
                InterpreterId = interpreterId,
                Status = ResponseStatus,
                Notes = Notes,
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            _db.InterpreterResponses.Add(response);
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Your response '{ResponseStatus}' has been recorded. Thank you!";
        return RedirectToPage();
    }
}
