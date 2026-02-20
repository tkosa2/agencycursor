using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Interpreters;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;

    public EditModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Interpreter Interpreter { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var i = await _db.Interpreters.FindAsync(id);
        if (i == null) return NotFound();
        Interpreter = i;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var existing = await _db.Interpreters.FirstOrDefaultAsync(i => i.Id == Interpreter.Id);
        if (existing == null) return NotFound();

        existing.Name = Interpreter.Name;
        existing.Languages = Interpreter.Languages;
        existing.Availability = Interpreter.Availability;
        existing.HomePhone = Interpreter.HomePhone;
        existing.BusinessPhone = Interpreter.BusinessPhone;
        existing.MobilePhone = Interpreter.MobilePhone;
        existing.Email = Interpreter.Email;
        existing.AddressLine1 = Interpreter.AddressLine1;
        existing.AddressLine2 = Interpreter.AddressLine2;
        existing.City = Interpreter.City;
        existing.State = Interpreter.State;
        existing.ZipCode = Interpreter.ZipCode;
        existing.Certification = Interpreter.Certification;
        existing.Notes = Interpreter.Notes;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Interpreters.AnyAsync(x => x.Id == Interpreter.Id)) return NotFound();
            throw;
        }
        return RedirectToPage("Index", new { registeredOnly = true });
    }
}
