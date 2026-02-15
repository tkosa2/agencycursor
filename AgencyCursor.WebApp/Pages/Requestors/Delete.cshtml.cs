using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Requestors;

public class DeleteModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DeleteModel(AgencyDbContext db) => _db = db;

    public Requestor? Requestor { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Requestor = await _db.Requestors.FindAsync(id);
        return Requestor == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();
        var r = await _db.Requestors.FindAsync(id);
        if (r != null)
        {
            _db.Requestors.Remove(r);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
