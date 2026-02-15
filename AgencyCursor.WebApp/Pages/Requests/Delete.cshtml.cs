using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class DeleteModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DeleteModel(AgencyDbContext db) => _db = db;

    public new Request? Request { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Request = await _db.Requests.Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == id);
        return Request == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();
        var r = await _db.Requests.FindAsync(id);
        if (r != null)
        {
            _db.Requests.Remove(r);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
