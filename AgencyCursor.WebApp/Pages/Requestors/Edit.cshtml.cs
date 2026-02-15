using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requestors;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;

    public EditModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Requestor Requestor { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var r = await _db.Requestors.FindAsync(id);
        if (r == null) return NotFound();
        Requestor = r;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
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
