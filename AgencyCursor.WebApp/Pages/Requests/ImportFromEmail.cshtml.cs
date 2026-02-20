using AgencyCursor.Data;
using AgencyCursor.Models;
using AgencyCursor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class ImportFromEmailModel : PageModel
{
    private readonly AgencyDbContext _db;

    public ImportFromEmailModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public string? EmailPlainText { get; set; }

    /// <summary>
    /// After parsing, holds extracted data for editing and creation.
    /// </summary>
    public ExtractedRequestFromEmail? Extracted { get; set; }

    [BindProperty]
    public CreateFromEmailInput CreateInput { get; set; } = new();

    public string? ParseError { get; set; }

    public IActionResult OnGet() => Page();

    public IActionResult OnPostParse()
    {
        if (string.IsNullOrWhiteSpace(EmailPlainText))
        {
            ParseError = "Please paste the request email text.";
            return Page();
        }
        Extracted = RequestEmailParser.Parse(EmailPlainText);
        CreateInput = new CreateFromEmailInput
        {
            RequestorName = Extracted.RequestorName,
            Phone = Extracted.Phone,
            Email = Extracted.Email,
            Address = Extracted.Address,
            TypeOfService = Extracted.TypeOfService,
            Location = Extracted.Location,
            ServiceDate = Extracted.ServiceDate,
            StartTime = Extracted.StartTime,
            PreferredInterpreterName = Extracted.PreferredInterpreterName,
            AdditionalNotes = Extracted.CombinedNotes
        };
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(CreateInput.RequestorName))
        {
            ModelState.AddModelError("CreateInput.RequestorName", "Requestor name is required.");
            Extracted = new ExtractedRequestFromEmail();
            return Page();
        }

        var requestor = await _db.Requestors
            .FirstOrDefaultAsync(r => r.Email != null && r.Email == CreateInput.Email);
        if (requestor == null)
        {
            requestor = new Requestor
            {
                Name = CreateInput.RequestorName.Trim(),
                Phone = CreateInput.Phone?.Trim(),
                Email = CreateInput.Email?.Trim(),
                Address = CreateInput.Address?.Trim()
            };
            _db.Requestors.Add(requestor);
            await _db.SaveChangesAsync();
        }
        else
        {
            requestor.Name = CreateInput.RequestorName.Trim();
            requestor.Phone = CreateInput.Phone?.Trim();
            requestor.Address = CreateInput.Address?.Trim();
            await _db.SaveChangesAsync();
        }

        var serviceDateTime = CreateInput.ServiceDate ?? DateTime.Today;
        if (!string.IsNullOrWhiteSpace(CreateInput.StartTime) && ParseTime(CreateInput.StartTime) is { } start)
            serviceDateTime = serviceDateTime.Date + start;
        else
            serviceDateTime = serviceDateTime.Date.AddHours(9);

        int? preferredInterpreterId = null;
        if (!string.IsNullOrWhiteSpace(CreateInput.PreferredInterpreterName))
        {
            var interpreter = await _db.Interpreters
                .FirstOrDefaultAsync(i => i.Name.Contains(CreateInput.PreferredInterpreterName.Trim()));
            if (interpreter != null)
                preferredInterpreterId = interpreter.Id;
        }

        var request = new Request
        {
            RequestorId = requestor.Id,
            TypeOfService = CreateInput.TypeOfService?.Trim(),
            Location = CreateInput.Location?.Trim(),
            ServiceDateTime = serviceDateTime,
            PreferredInterpreterId = preferredInterpreterId,
            AdditionalNotes = CreateInput.AdditionalNotes?.Trim(),
            Status = "New Request"
        };
        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        return RedirectToPage("Details", new { id = request.Id });
    }

    private static TimeSpan? ParseTime(string? timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr)) return null;
        timeStr = timeStr.Trim();
        if (DateTime.TryParseExact(timeStr, new[] { "h:mm tt", "h:m tt", "hh:mm tt", "H:mm" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
            return dt.TimeOfDay;
        if (DateTime.TryParseExact(timeStr, new[] { "h:mm tt", "h:m tt", "hh:mm tt", "H:mm" }, new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out dt))
            return dt.TimeOfDay;
        return null;
    }
}

public class CreateFromEmailInput
{
    public string? RequestorName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TypeOfService { get; set; }
    public string? Location { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? StartTime { get; set; }
    public string? PreferredInterpreterName { get; set; }
    public string? AdditionalNotes { get; set; }
}
