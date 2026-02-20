using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgencyCursor.Pages.Requests;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly ILogger<EditModel> _logger;

    public EditModel(AgencyDbContext db, ILogger<EditModel> logger)
    {
        _db = db;
        _logger = logger;
    }

    [BindProperty]
    public new Request Request { get; set; } = null!;

    public SelectList RequestorList { get; set; } = null!;
    public SelectList StateList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var r = await _db.Requests
            .Include(req => req.Requestor)
            .FirstOrDefaultAsync(req => req.Id == id);
        if (r == null) return NotFound();
        Request = r;
        RequestorList = new SelectList(await _db.Requestors.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", Request.RequestorId);
        
        // Get unique states from ZipCode table
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
        StateList = new SelectList(states, Request.State);
        
        // If PreferredInterpreterName is empty but PreferredInterpreterId is set, populate the name from the interpreter
        if (string.IsNullOrEmpty(Request.PreferredInterpreterName) && Request.PreferredInterpreterId.HasValue)
        {
            var interpreter = await _db.Interpreters.FindAsync(Request.PreferredInterpreterId.Value);
            if (interpreter != null)
            {
                Request.PreferredInterpreterName = interpreter.Name;
            }
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Read Specializations from form
        var postedSpecializations = HttpContext.Request.Form["Specializations"].ToArray();
        if (postedSpecializations != null && postedSpecializations.Length > 0)
        {
            Request.Specializations = string.Join(", ", postedSpecializations);
        }
        else
        {
            Request.Specializations = null;
        }
        
        // Read form values directly FIRST before any validation
        // This ensures we capture the posted data before ModelState validation interferes
        var postedRequestorIdValue = HttpContext.Request.Form["Request.RequestorId"].ToString();
        int postedRequestorId = 0;
        if (!string.IsNullOrEmpty(postedRequestorIdValue))
        {
            int.TryParse(postedRequestorIdValue, out postedRequestorId);
        }
        
        // Read EndDateTime if provided
        var postedEndDateTimeValue = HttpContext.Request.Form["Request.EndDateTime"].ToString();
        DateTime? postedEndDateTime = null;
        if (!string.IsNullOrEmpty(postedEndDateTimeValue))
        {
            if (DateTime.TryParse(postedEndDateTimeValue, out var endDateTime))
            {
                postedEndDateTime = endDateTime;
            }
        }
        
        var postedDateTimeValue = HttpContext.Request.Form["Request.ServiceDateTime"].ToString();
        DateTime postedDateTime = default;
        if (!string.IsNullOrEmpty(postedDateTimeValue))
        {
            // datetime-local format can be "yyyy-MM-ddTHH:mm" or "yyyy-MM-ddTHH:mm:ss" (no timezone)
            // Try parsing with specific formats first
            string[] formats = { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" };
            bool parsed = false;
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(postedDateTimeValue, format, null, System.Globalization.DateTimeStyles.None, out postedDateTime))
                {
                    parsed = true;
                    break;
                }
            }
            // Fallback to standard parsing if format-specific parsing failed
            if (!parsed)
            {
                DateTime.TryParse(postedDateTimeValue, out postedDateTime);
            }
        }
        
        // Remove ModelState errors BEFORE we use the values
        // This prevents the [Range] and [Required] attributes from interfering with binding
        // We'll validate manually after reading the form values
        ModelState.Remove("Request.RequestorId");
        ModelState.Remove("Request.Requestor"); // Remove navigation property validation
        ModelState.Remove("Request.ServiceDateTime");
        ModelState.Remove("Request.EndDateTime");
        ModelState.Remove("Request.PreferredInterpreterId");
        ModelState.Remove("Request.PreferredInterpreterName");
        ModelState.Remove("Request.Specializations");
        
        // Use the form values directly to ensure we have the posted data
        // Always set RequestorId from the form value
        Request.RequestorId = postedRequestorId;
        
        // Use posted datetime if valid, otherwise try to use the bound value
        if (postedDateTime != default(DateTime))
        {
            Request.ServiceDateTime = postedDateTime;
        }
        // If form parsing failed, the bound value should already be set if binding succeeded
        // We'll validate it below to ensure it's not default
        
        // Set EndDateTime if provided
        if (postedEndDateTime.HasValue)
        {
            Request.EndDateTime = postedEndDateTime;
        }
        
        // Debug: Log what we have (can be removed later)
        System.Diagnostics.Debug.WriteLine($"Posted DateTime Value: '{postedDateTimeValue}'");
        System.Diagnostics.Debug.WriteLine($"Parsed DateTime: {postedDateTime}");
        System.Diagnostics.Debug.WriteLine($"Bound DateTime: {Request.ServiceDateTime}");
        System.Diagnostics.Debug.WriteLine($"IsValid: {ModelState.IsValid}");
        _logger.LogInformation($"Posted DateTime Value: '{postedDateTimeValue}'");
        _logger.LogInformation($"Parsed DateTime: {postedDateTime}");
        _logger.LogInformation($"Bound DateTime: {Request.ServiceDateTime}");
        _logger.LogInformation($"IsValid: {ModelState.IsValid}");
        
        // Log all ModelState errors to help debug
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    _logger.LogWarning($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
        }
        
        // Validate ServiceDateTime
        if (Request.ServiceDateTime == default(DateTime))
        {
            ModelState.AddModelError("Request.ServiceDateTime", "Date and time is required.");
        }
        
        // Try to find preferred interpreter by name if provided
        if (!string.IsNullOrEmpty(Request.PreferredInterpreterName))
        {
            var interpreter = await _db.Interpreters
                .FirstOrDefaultAsync(i => i.Name.Contains(Request.PreferredInterpreterName));
            if (interpreter != null)
            {
                Request.PreferredInterpreterId = interpreter.Id;
            }
            else
            {
                // No match found, clear the ID but keep the name
                Request.PreferredInterpreterId = null;
            }
        }
        // If name is cleared, also clear the ID
        else
        {
            Request.PreferredInterpreterId = null;
        }
        
        // Get unique states from ZipCode table for reload
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
        StateList = new SelectList(states, Request?.State);
        
        // Reload dropdowns if validation fails
        if (!ModelState.IsValid)
        {
            RequestorList = new SelectList(await _db.Requestors.OrderBy(r => r.Name).ToListAsync(), "Id", "Name", Request.RequestorId);
            return Page();
        }

        // Load the existing request with tracking
        var existingRequest = await _db.Requests.FindAsync(Request.Id);
        if (existingRequest == null)
        {
            return NotFound();
        }

        // Update all properties from the bound model
        existingRequest.RequestorId = Request.RequestorId;
        existingRequest.RequestName = Request.RequestName;
        existingRequest.NumberOfIndividuals = Request.NumberOfIndividuals;
        existingRequest.IndividualType = Request.IndividualType;
        existingRequest.TypeOfService = Request.TypeOfService;
        existingRequest.TypeOfServiceOther = Request.TypeOfServiceOther;
        existingRequest.Mode = Request.Mode;
        existingRequest.MeetingLink = Request.MeetingLink;
        existingRequest.Address = Request.Address;
        existingRequest.Address2 = Request.Address2;
        existingRequest.City = Request.City;
        existingRequest.State = Request.State;
        existingRequest.ZipCode = Request.ZipCode;
        existingRequest.GenderPreference = Request.GenderPreference;
        existingRequest.PreferredInterpreterName = Request.PreferredInterpreterName;
        existingRequest.PreferredInterpreterId = Request.PreferredInterpreterId;
        existingRequest.ConsumerNames = Request.ConsumerNames;
        existingRequest.Specializations = Request.Specializations;
        existingRequest.InternationalOther = Request.InternationalOther;
        existingRequest.OtherInterpreter = Request.OtherInterpreter;
        existingRequest.Location = Request.Location;
        existingRequest.AdditionalNotes = Request.AdditionalNotes;
        existingRequest.Status = Request.Status;
        
        // Explicitly set ServiceDateTime - this is the key fix
        // The datetime-local input sends local time, we store it as-is
        existingRequest.ServiceDateTime = Request.ServiceDateTime;
        
        // Minimum 2-hour interpreter request - always set EndDateTime to 2 hours after start
        existingRequest.EndDateTime = Request.ServiceDateTime.AddHours(2);
        
        // Mark the entity as modified to ensure EF tracks all changes
        _db.Entry(existingRequest).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Requests.AnyAsync(x => x.Id == Request.Id)) return NotFound();
            throw;
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while saving: {ex.Message}");
            RequestorList = new SelectList(await _db.Requestors.OrderBy(r => r.Name).ToListAsync(), "Id", "Name", Request.RequestorId);
            var statesForError = await _db.ZipCodes
                .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
                .Select(z => z.AdminCode1)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            StateList = new SelectList(statesForError, Request?.State);
            return Page();
        }
        return RedirectToPage("Index");
    }
}
