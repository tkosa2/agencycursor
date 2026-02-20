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

    public List<Interpreter> RegisteredInterpreters { get; set; } = new();
    public async Task OnGetAsync()
    {
        RegisteredInterpreters = await _db.Interpreters
            .OrderBy(i => i.Name)
            .ToListAsync();
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
