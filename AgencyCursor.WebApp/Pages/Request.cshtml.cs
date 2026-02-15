using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Pages;

public class RequestModel : PageModel
{
    private readonly AgencyDbContext _db;

    public RequestModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Models.Request Request { get; set; } = null!;

    [BindProperty, Required(ErrorMessage = "Phone number is required.")]
    [Phone]
    public string RequestorPhone { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Email address is required.")]
    [EmailAddress]
    public string RequestorEmail { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Appointment date is required.")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    [BindProperty, Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [BindProperty, Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    [BindProperty]
    public string[]? Specializations { get; set; }

    public SelectList StateList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        // Get unique states from ZipCode table
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        StateList = new SelectList(states, Request?.State);

        Request = new Models.Request
        {
            NumberOfIndividuals = 1,
            IndividualType = "deaf",
            Mode = "In-Person",
            GenderPreference = "none",
            Status = "Pending"
        };
        AppointmentDate = DateTime.Today;
        StartTime = new TimeSpan(9, 0, 0);
        EndTime = new TimeSpan(10, 0, 0);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear ModelState errors for Request object to add our own validation
        ModelState.Remove("Request.RequestName");
        ModelState.Remove("Request.ServiceDateTime");
        ModelState.Remove("Request.TypeOfService");
        ModelState.Remove("Request.Mode");
        ModelState.Remove("Request.RequestorId"); // We'll set this after creating the requestor
        
        // Validate required fields explicitly
        bool hasErrors = false;

        if (string.IsNullOrWhiteSpace(Request.RequestName))
        {
            ModelState.AddModelError("Request.RequestName", "Request name is required.");
            hasErrors = true;
        }

        if (AppointmentDate == default(DateTime) || AppointmentDate < DateTime.Now.AddYears(-10))
        {
            ModelState.AddModelError("AppointmentDate", "Please select a valid appointment date.");
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(RequestorPhone))
        {
            ModelState.AddModelError("RequestorPhone", "Phone number is required.");
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(RequestorEmail) || !RequestorEmail.Contains("@"))
        {
            ModelState.AddModelError("RequestorEmail", "A valid email address is required.");
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(Request.TypeOfService))
        {
            ModelState.AddModelError("Request.TypeOfService", "Please select a type of appointment.");
            hasErrors = true;
        }

        // Validate location fields for in-person appointments
        if (Request.Mode == "In-Person")
        {
            if (string.IsNullOrWhiteSpace(Request.Address))
            {
                ModelState.AddModelError("Request.Address", "Address is required for in-person appointments.");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(Request.City))
            {
                ModelState.AddModelError("Request.City", "City is required for in-person appointments.");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(Request.State))
            {
                ModelState.AddModelError("Request.State", "State is required for in-person appointments.");
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(Request.ZipCode))
            {
                ModelState.AddModelError("Request.ZipCode", "ZIP code is required for in-person appointments.");
                hasErrors = true;
            }
        }

        if (hasErrors)
        {
            // Reload state list
            var states = await _db.ZipCodes
                .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
                .Select(z => z.AdminCode1)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            StateList = new SelectList(states, Request?.State);
            return Page();
        }

        // Always create a new requestor for public submissions
        var requestor = new Requestor
        {
            Name = Request.RequestName ?? "Unknown",
            Phone = RequestorPhone,
            Email = RequestorEmail,
            Address = BuildAddressString()
        };
        _db.Requestors.Add(requestor);
        await _db.SaveChangesAsync();

        // Set Request properties before adding to database
        Request.RequestorId = requestor.Id;
        
        // Combine date and time for ServiceDateTime
        Request.ServiceDateTime = AppointmentDate.Date.Add(StartTime);
        Request.EndDateTime = AppointmentDate.Date.Add(EndTime);
        
        // Always set Status to Pending for new requests
        Request.Status = "Pending";

        // Set Location based on Mode
        if (Request.Mode == "Virtual")
        {
            Request.Location = "Virtual";
        }
        else
        {
            Request.Location = BuildAddressString();
        }

        // Handle specializations
        if (Specializations != null && Specializations.Length > 0)
        {
            Request.Specializations = string.Join(", ", Specializations);
        }

        // Try to find preferred interpreter by name if provided
        if (!string.IsNullOrEmpty(Request.PreferredInterpreterName) && Request.PreferredInterpreterId == null)
        {
            var interpreter = await _db.Interpreters
                .FirstOrDefaultAsync(i => i.Name.Contains(Request.PreferredInterpreterName));
            if (interpreter != null)
            {
                Request.PreferredInterpreterId = interpreter.Id;
            }
        }

        _db.Requests.Add(Request);
        await _db.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "Your request has been submitted successfully! We will contact you soon.";
        return RedirectToPage("/Request");
    }

    private string BuildAddressString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(Request.Address))
            parts.Add(Request.Address);
        if (!string.IsNullOrEmpty(Request.Address2))
            parts.Add(Request.Address2);
        if (!string.IsNullOrEmpty(Request.City))
            parts.Add(Request.City);
        if (!string.IsNullOrEmpty(Request.State))
            parts.Add(Request.State);
        if (!string.IsNullOrEmpty(Request.ZipCode))
            parts.Add(Request.ZipCode);
        return string.Join(", ", parts);
    }
}
