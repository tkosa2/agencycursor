using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Pages.Requests;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public new Request Request { get; set; } = null!;

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

    [BindProperty]
    public string RequestorFirstName { get; set; } = string.Empty;

    [BindProperty]
    public string RequestorLastName { get; set; } = string.Empty;

    public SelectList RequestorList { get; set; } = null!;
    public SelectList InterpreterList { get; set; } = null!;
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

        StateList = new SelectList(states);

        RequestorList = new SelectList(_db.Requestors.OrderBy(r => r.Name), "Id", "Name");
        InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name", null);
        Request = new Request
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
        // Remove RequestorId validation since we'll set it after creating/finding the requestor
        ModelState.Remove("Request.RequestorId");
        ModelState.Remove("Request.Requestor");
        ModelState.Remove("Request.RequestName"); // Auto-generated from FirstName and LastName
        
        // Get unique states from ZipCode table
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        StateList = new SelectList(states, Request?.State);

        if (!ModelState.IsValid)
        {
            // Add a general error message if validation fails
            if (ModelState.ErrorCount > 0)
            {
                TempData["ErrorMessage"] = "Please correct the errors below and try again.";
            }
            
            RequestorList = new SelectList(_db.Requestors.OrderBy(r => r.Name), "Id", "Name");
            InterpreterList = new SelectList(_db.Interpreters.OrderBy(i => i.Name), "Id", "Name", Request.PreferredInterpreterId);
            return Page();
        }

        // Find or create requestor
        var requestor = await _db.Requestors
            .FirstOrDefaultAsync(r => r.Email == RequestorEmail || r.Phone == RequestorPhone);

        // Combine first and last name
        var fullName = string.IsNullOrWhiteSpace(RequestorFirstName) && string.IsNullOrWhiteSpace(RequestorLastName)
            ? "Unknown"
            : $"{RequestorFirstName} {RequestorLastName}".Trim();

        if (requestor == null)
        {
            // Create new requestor
            requestor = new Requestor
            {
                FirstName = RequestorFirstName,
                LastName = RequestorLastName,
                Name = fullName,
                Phone = RequestorPhone,
                Email = RequestorEmail,
                Address = BuildAddressString()
            };
            _db.Requestors.Add(requestor);
            await _db.SaveChangesAsync();
        }
        else
        {
            // Update existing requestor if needed
            if (string.IsNullOrEmpty(requestor.Phone) && !string.IsNullOrEmpty(RequestorPhone))
                requestor.Phone = RequestorPhone;
            if (string.IsNullOrEmpty(requestor.Email) && !string.IsNullOrEmpty(RequestorEmail))
                requestor.Email = RequestorEmail;
            if (string.IsNullOrEmpty(requestor.Address))
                requestor.Address = BuildAddressString();
            // Update name fields if not set
            if (string.IsNullOrEmpty(requestor.FirstName) && !string.IsNullOrEmpty(RequestorFirstName))
                requestor.FirstName = RequestorFirstName;
            if (string.IsNullOrEmpty(requestor.LastName) && !string.IsNullOrEmpty(RequestorLastName))
                requestor.LastName = RequestorLastName;
            if (string.IsNullOrEmpty(requestor.Name) || requestor.Name == "Unknown")
                requestor.Name = fullName;
        }

        // Set Request name fields
        Request.FirstName = RequestorFirstName;
        Request.LastName = RequestorLastName;
        Request.RequestName = fullName;

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
        return RedirectToPage("Index");
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
