using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Interpreters;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(AgencyDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _environment = environment;
    }

    [BindProperty(SupportsGet = true)]
    public string? FirstName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? LastName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? City { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? State { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ZipCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FreelanceStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Gender { get; set; }

    public List<Dictionary<string, object?>> RidSearchResults { get; set; } = new();
    public List<Interpreter> RegisteredInterpreters { get; set; } = new();
    public bool HasSearched { get; set; }
    public bool ShowRegisteredOnly { get; set; } = false;

    public async Task OnGetAsync(bool registeredOnly = false)
    {
        ShowRegisteredOnly = registeredOnly;

        if (registeredOnly)
        {
            // Show only registered interpreters from agency.db
            RegisteredInterpreters = await _db.Interpreters
                .Where(i => i.IsRegisteredWithAgency)
                .OrderBy(i => i.Name)
                .ToListAsync();
        }
        else
        {
            // Check if any search parameters were provided
            HasSearched = !string.IsNullOrWhiteSpace(FirstName) ||
                          !string.IsNullOrWhiteSpace(LastName) ||
                          !string.IsNullOrWhiteSpace(Email) ||
                          !string.IsNullOrWhiteSpace(City) ||
                          !string.IsNullOrWhiteSpace(State) ||
                          !string.IsNullOrWhiteSpace(ZipCode) ||
                          !string.IsNullOrWhiteSpace(Category) ||
                          !string.IsNullOrWhiteSpace(FreelanceStatus) ||
                          !string.IsNullOrWhiteSpace(Gender);

            if (HasSearched)
            {
                var ridDbPath = RidDbHelper.GetRidDbPath(_environment.ContentRootPath);
                if (System.IO.File.Exists(ridDbPath))
                {
                    RidSearchResults = await RidInterpreterService.SearchInterpretersAsync(
                        ridDbPath,
                        firstName: FirstName,
                        lastName: LastName,
                        email: Email,
                        city: City,
                        state: State,
                        zipCode: ZipCode,
                        category: Category,
                        freelanceStatus: FreelanceStatus,
                        gender: Gender
                    );
                }
            }
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync(string interpreterData)
    {
        if (string.IsNullOrWhiteSpace(interpreterData))
        {
            return BadRequest();
        }

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var ridInterpreterData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(interpreterData, options);
            if (ridInterpreterData == null)
            {
                return BadRequest();
            }

            var ridDbPath = RidDbHelper.GetRidDbPath(_environment.ContentRootPath);
            var interpreter = await InterpreterRegistrationService.ImportInterpreterFromRidAsync(
                _db,
                ridDbPath,
                ridInterpreterData
            );

            if (interpreter != null)
            {
                TempData["SuccessMessage"] = $"Successfully registered {interpreter.Name} with the agency.";
                return RedirectToPage(new { registeredOnly = true });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to register interpreter. Please try again.";
                return RedirectToPage();
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToPage();
        }
    }

    [BindProperty(SupportsGet = true)]
    public string? BulkState { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? BulkCity { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? BulkLimit { get; set; }

    public async Task<IActionResult> OnPostBulkRegisterAsync(string? state, string? city, int? limit)
    {
        try
        {
            var ridDbPath = Path.Combine(_environment.ContentRootPath, "rid_interpreters.db");
            if (!System.IO.File.Exists(ridDbPath))
            {
                TempData["ErrorMessage"] = "RID database not found.";
                return RedirectToPage(new { registeredOnly = true });
            }

            var result = await InterpreterRegistrationService.SearchAndRegisterInterpretersAsync(
                _db,
                ridDbPath,
                limit: limit,
                state: state,
                city: city
            );

            if (result.Success)
            {
                var message = $"Bulk registration completed: {result.Created} created, {result.Registered} registered, {result.AlreadyRegistered} already registered, {result.Skipped} skipped, {result.Errors} errors.";
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Bulk registration failed.";
            }

            return RedirectToPage(new { registeredOnly = true });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error during bulk registration: {ex.Message}";
            return RedirectToPage(new { registeredOnly = true });
        }
    }
}
