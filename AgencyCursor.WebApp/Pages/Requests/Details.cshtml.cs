using AgencyCursor.Data;
using AgencyCursor.Models;
using AgencyCursor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly IEmailService _emailService;

    public DetailsModel(AgencyDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public new Request? Request { get; set; }
    public IList<Appointment> Appointments { get; set; } = new List<Appointment>();
    public IList<Interpreter> Interpreters { get; set; } = new List<Interpreter>();
    public IList<InterpreterResponse> InterpreterResponses { get; set; } = new List<InterpreterResponse>();
    public Dictionary<int, InterpreterResponse?> ResponsesByInterpreterId { get; set; } = new Dictionary<int, InterpreterResponse?>();
    public IList<InterpreterEmailLog> EmailLogs { get; set; } = new List<InterpreterEmailLog>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Request = await _db.Requests
            .Include(r => r.Requestor)
            .Include(r => r.PreferredInterpreter)
            .Include(r => r.Appointments)
            .ThenInclude(a => a.Interpreter)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (Request == null) return NotFound();
        Appointments = Request.Appointments.OrderByDescending(a => a.ServiceDateTime).ToList();
        
        // Load interpreters for the modal
        await LoadInterpretersAsync();

        // Load responses for the interpreters
        await LoadInterpreterResponsesAsync(id.Value);

        // Load email logs for this request
        await LoadEmailLogsAsync(id.Value);
        
        return Page();
    }

    private async Task LoadInterpretersAsync()
    {
        var query = _db.Interpreters.Where(i => !string.IsNullOrEmpty(i.Email)).AsQueryable();

        // Filter by specialization if specified
        if (!string.IsNullOrEmpty(Request?.Specializations))
        {
            var requestSpecializations = Request.Specializations.Split(',')
                .Select(s => s.Trim().ToLower())
                .ToList();

            Interpreters = await query
                .Where(i => requestSpecializations.Any(s => 
                    (i.Languages ?? "").ToLower().Contains(s) ||
                    (i.Certification ?? "").ToLower().Contains(s)))
                .OrderBy(i => i.Name)
                .ToListAsync();
        }
        else
        {
            // If no specializations specified, show all interpreters with email
            Interpreters = await query.OrderBy(i => i.Name).ToListAsync();
        }
    }

    private async Task LoadInterpreterResponsesAsync(int requestId)
    {
        InterpreterResponses = await _db.InterpreterResponses
            .Where(ir => ir.RequestId == requestId)
            .Include(ir => ir.Interpreter)
            .OrderByDescending(ir => ir.RespondedAt)
            .ToListAsync();

        // Create a dictionary for quick lookup
        ResponsesByInterpreterId = InterpreterResponses
            .GroupBy(ir => ir.InterpreterId)
            .ToDictionary(g => g.Key, g => g.FirstOrDefault());
    }

    private async Task LoadEmailLogsAsync(int requestId)
    {
        EmailLogs = await _db.InterpreterEmailLogs
            .Where(el => el.RequestId == requestId)
            .Include(el => el.Interpreter)
            .OrderByDescending(el => el.SentAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostNotifyInterpretersAsync(int requestId, int[]? selectedInterpreterIds, string? customMessage)
    {
        Request = await _db.Requests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (Request == null) return NotFound();

        if (selectedInterpreterIds == null || !selectedInterpreterIds.Any())
        {
            TempData["ErrorMessage"] = "Please select at least one interpreter to notify.";
            return RedirectToPage(new { id = requestId });
        }

        var selectedInterpreters = await _db.Interpreters
            .Where(i => selectedInterpreterIds.Contains(i.Id) && !string.IsNullOrEmpty(i.Email))
            .ToListAsync();

        if (!selectedInterpreters.Any())
        {
            TempData["ErrorMessage"] = "Selected interpreters do not have email addresses.";
            return RedirectToPage(new { id = requestId });
        }

        try
        {
            // Send personalized emails to each interpreter with their unique response URL
            foreach (var interpreter in selectedInterpreters)
            {
                if (string.IsNullOrEmpty(interpreter.Email))
                    continue;

                var personalizedEmail = BuildNotificationEmail(customMessage, interpreter.Id);
                await _emailService.SendEmailAsync(interpreter.Email,
                    $"New Interpretation Request Available - #{Request.Id}",
                    personalizedEmail,
                    requestId: Request.Id,
                    interpreterId: interpreter.Id);
            }

            // Update request status to "Broadcasted"
            Request.Status = "Broadcasted";
            _db.Requests.Update(Request);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Notifications sent to {selectedInterpreters.Count} interpreter(s). Request status updated to 'Broadcasted'.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error sending notifications: {ex.Message}";
        }

        return RedirectToPage(new { id = requestId });
    }

    private string BuildNotificationEmail(string? customMessage, int interpreterId)
    {
        var responseUrl = $"https://{HttpContext.Request.Host}/Interpreters/RespondToRequest/{Request.Id}/{interpreterId}";
        
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 5px; overflow: hidden; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; }}
        .content {{ padding: 20px; }}
        .details {{ background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 15px 0; }}
        .details strong {{ display: block; margin-bottom: 5px; }}
        .footer {{ background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #ddd; }}
        .cta-button {{ display: inline-block; background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 15px 0; font-weight: bold; }}
        .response-options {{ margin: 20px 0; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h2>New Interpretation Request Available</h2>
            <p>Request ID: #{Request.Id}</p>
        </div>
        <div class=""content"">
            <p>Hello,</p>
            <p>A new interpretation request matching your qualifications is available. Please review the details and let us know if you're interested!</p>
            
            <div class=""details"">
                <strong>Service Details:</strong>
                Service Type: {(Request.TypeOfService ?? "—")}{(!string.IsNullOrEmpty(Request.TypeOfServiceOther) ? $" ({Request.TypeOfServiceOther})" : "")}<br />
                Date & Time: {Request.ServiceDateTime:g}<br />
                Duration: 2 hours (est.)<br />
                Mode: {(Request.Mode ?? "—")}<br />
                Location: {(Request.Location ?? "—")}<br />
                <br />
                <strong>Required Specializations:</strong><br />
                {(!string.IsNullOrEmpty(Request.Specializations) ? Request.Specializations : "—")}
            </div>

            {(Request.NumberOfIndividuals > 1 ? $"<div class=\"details\"><strong>Number of Individuals:</strong> {Request.NumberOfIndividuals}</div>" : "")}

            {(!string.IsNullOrEmpty(Request.ConsumerNames) ? $@"
            <div class=""details"">
                <strong>Consumer Name(s):</strong>
                {Request.ConsumerNames}
            </div>" : "")}

            {(!string.IsNullOrEmpty(Request.GenderPreference) ? $@"
            <div class=""details"">
                <strong>Gender Preference:</strong>
                {Request.GenderPreference}
            </div>" : "")}

            {(!string.IsNullOrEmpty(customMessage) ? $@"
            <div class=""details"">
                <strong>Additional Notes:</strong>
                {System.Security.SecurityElement.Escape(customMessage)}
            </div>" : "")}

            <div class=""response-options"">
                <p><strong>Ready to respond? Click the button below:</strong></p>
                <a href=""{responseUrl}"" class=""cta-button"">Tell Us Your Response</a>
                <p style=""font-size: 12px; color: #666;"">You can respond with: <strong>Yes</strong> (interested), <strong>No</strong> (not available), or <strong>Maybe</strong> (need info)</p>
            </div>

            <p><em>If you have any questions about this request, please reply to this email or click the button above to view full details and provide your response.</em></p>

            <p>Best regards,<br />Agency Cursor Team</p>
        </div>
        <div class=""footer"">
            <p>This is an automated message. Each interpreter will see their own unique response page.</p>
        </div>
    </div>
</body>
</html>";
        return html;
    }
}

