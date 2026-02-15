using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Invoices;

public class DeleteModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DeleteModel(AgencyDbContext db) => _db = db;

    public Invoice? Invoice { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Invoice = await _db.Invoices
            .Include(i => i.Requestor)
            .Include(i => i.Interpreter)
            .FirstOrDefaultAsync(i => i.Id == id);
        return Invoice == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();
        var inv = await _db.Invoices.FindAsync(id);
        if (inv != null)
        {
            _db.Invoices.Remove(inv);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
