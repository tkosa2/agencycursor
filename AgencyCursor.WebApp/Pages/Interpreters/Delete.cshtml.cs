using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Interpreters;

public class DeleteModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DeleteModel(AgencyDbContext db) => _db = db;

    public Interpreter? Interpreter { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Interpreter = await _db.Interpreters.FindAsync(id);
        return Interpreter == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();
        var i = await _db.Interpreters.FindAsync(id);
        if (i != null)
        {
            _db.Interpreters.Remove(i);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
