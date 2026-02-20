using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requestors;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;

    public EditModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Requestor Requestor { get; set; } = null!;

    public SelectList StateList { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var r = await _db.Requestors.FindAsync(id);
        if (r == null) return NotFound();
        Requestor = r;
        
        // Get unique states from ZipCode table
        var states = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1) && z.CountryCode == "US")
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        StateList = new SelectList(states, Requestor?.State);
        
        // If FirstName/LastName are empty but Name exists, try to split Name
        if (string.IsNullOrWhiteSpace(Requestor.FirstName) && string.IsNullOrWhiteSpace(Requestor.LastName) && 
            !string.IsNullOrWhiteSpace(Requestor.Name))
        {
            var nameParts = Requestor.Name.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length > 0)
            {
                Requestor.FirstName = nameParts[0];
                if (nameParts.Length > 1)
                {
                    Requestor.LastName = nameParts[1];
                }
            }
        }
        
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
        _db.Attach(Requestor).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Requestors.AnyAsync(x => x.Id == Requestor.Id)) return NotFound();
            throw;
        }
        return RedirectToPage("Index");
    }
}
