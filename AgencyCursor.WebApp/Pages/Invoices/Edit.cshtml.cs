using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Invoices;

public class EditModel : PageModel
{
    private readonly AgencyDbContext _db;

    public EditModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Invoice Invoice { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        var inv = await _db.Invoices.FindAsync(id);
        if (inv == null) return NotFound();
        Invoice = inv;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        if (Invoice.HourlyRate > 0 && Invoice.HoursWorked > 0)
            Invoice.TotalCost = Invoice.HoursWorked * Invoice.HourlyRate - Invoice.Discount;
        _db.Attach(Invoice).State = EntityState.Modified;
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Invoices.AnyAsync(x => x.Id == Invoice.Id)) return NotFound();
            throw;
        }
        return RedirectToPage("Index");
    }
}
