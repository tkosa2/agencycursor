using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requestors;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Requestor Requestor { get; set; } = null!;

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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Get unique states from ZipCode table
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        StateList = new SelectList(states, Requestor?.State);

        // Combine first and last name into Name if not already set
        if (string.IsNullOrWhiteSpace(Requestor.Name))
        {
            Requestor.Name = $"{Requestor.FirstName?.Trim()} {Requestor.LastName?.Trim()}".Trim();
        }
        // If Name is empty but FirstName or LastName exist, use them
        else if (string.IsNullOrWhiteSpace(Requestor.Name) && 
                 (!string.IsNullOrWhiteSpace(Requestor.FirstName) || !string.IsNullOrWhiteSpace(Requestor.LastName)))
        {
            Requestor.Name = $"{Requestor.FirstName?.Trim()} {Requestor.LastName?.Trim()}".Trim();
        }
        
        if (!ModelState.IsValid) return Page();
        _db.Requestors.Add(Requestor);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
