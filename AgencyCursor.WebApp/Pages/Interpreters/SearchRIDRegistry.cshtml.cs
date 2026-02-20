using AgencyCursor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Interpreters;

public class SearchRIDRegistryModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public SearchRIDRegistryModel(AgencyDbContext db, IWebHostEnvironment environment)
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
    public string? Phone { get; set; }

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
    public bool HasSearched { get; set; }

    public async Task OnGetAsync()
    {
        HasSearched = !string.IsNullOrWhiteSpace(FirstName) ||
                      !string.IsNullOrWhiteSpace(LastName) ||
                      !string.IsNullOrWhiteSpace(Email) ||
                      !string.IsNullOrWhiteSpace(Phone) ||
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
                    phone: Phone,
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

    public async Task<IActionResult> OnPostRegisterAsync(string? interpreterData, string? interpreterDataBase64)
    {
        if (string.IsNullOrWhiteSpace(interpreterData) && string.IsNullOrWhiteSpace(interpreterDataBase64))
        {
            return BadRequest();
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(interpreterDataBase64))
            {
                var decodedBytes = Convert.FromBase64String(interpreterDataBase64);
                interpreterData = System.Text.Encoding.UTF8.GetString(decodedBytes);
            }

            if (string.IsNullOrWhiteSpace(interpreterData))
            {
                return BadRequest();
            }

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
                return RedirectToPage("Index");
            }

            TempData["ErrorMessage"] = "Failed to register interpreter. Please try again.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
            return RedirectToPage();
        }
    }
}
