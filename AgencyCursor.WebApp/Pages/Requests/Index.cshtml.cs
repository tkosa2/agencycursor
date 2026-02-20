using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db) => _db = db;

    public IList<Request> Requests { get; set; } = new List<Request>();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public SelectList StatusOptions { get; set; } = null!;

    public void OnGet()
    {
        var statusOptions = new List<string>
        {
            "All",
            "New Request",
            "Reviewed",
            "Approved",
            "Broadcasted"
        };
        StatusOptions = new SelectList(statusOptions);

        var query = _db.Requests
            .Include(r => r.Requestor)
            .Include(r => r.PreferredInterpreter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(StatusFilter) && StatusFilter != "All")
        {
            query = query.Where(r => r.Status == StatusFilter);
        }

        Requests = query
            .OrderByDescending(r => r.ServiceDateTime)
            .ToList();
    }

    public async Task<IActionResult> OnPostCloneAsync(int? id)
    {
        if (id == null)
        {
            TempData["ErrorMessage"] = "Request ID is required.";
            return RedirectToPage();
        }

        var originalRequest = await _db.Requests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (originalRequest == null)
        {
            TempData["ErrorMessage"] = "Request not found.";
            return RedirectToPage();
        }

        // Create a clone of the request
        var clonedRequest = new Request
        {
            RequestorId = originalRequest.RequestorId,
            RequestName = originalRequest.RequestName,
            NumberOfIndividuals = originalRequest.NumberOfIndividuals,
            IndividualType = originalRequest.IndividualType,
            TypeOfService = originalRequest.TypeOfService,
            TypeOfServiceOther = originalRequest.TypeOfServiceOther,
            Mode = originalRequest.Mode,
            MeetingLink = originalRequest.MeetingLink,
            Address = originalRequest.Address,
            Address2 = originalRequest.Address2,
            City = originalRequest.City,
            State = originalRequest.State,
            ZipCode = originalRequest.ZipCode,
            GenderPreference = originalRequest.GenderPreference,
            PreferredInterpreterId = originalRequest.PreferredInterpreterId,
            PreferredInterpreterName = originalRequest.PreferredInterpreterName,
            Specializations = originalRequest.Specializations,
            ServiceDateTime = originalRequest.ServiceDateTime,
            EndDateTime = originalRequest.EndDateTime,
            Location = originalRequest.Location,
            Status = "New Request", // Reset status to New Request for cloned request
            AdditionalNotes = originalRequest.AdditionalNotes != null 
                ? $"Cloned from Request #{originalRequest.Id}. {originalRequest.AdditionalNotes}"
                : $"Cloned from Request #{originalRequest.Id}."
        };

        _db.Requests.Add(clonedRequest);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Request #{originalRequest.Id} has been cloned. New request ID: #{clonedRequest.Id}";
        return RedirectToPage();
    }
}
