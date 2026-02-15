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
        _db.Attach(Interpreter).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Interpreters.AnyAsync(x => x.Id == Interpreter.Id)) return NotFound();
            throw;
        }
        return RedirectToPage("Index");
    }
}
