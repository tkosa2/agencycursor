using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class DeleteModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DeleteModel(AgencyDbContext db) => _db = db;

    public Appointment? Appointment { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Appointment = await _db.Appointments
            .Include(a => a.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(a => a.Interpreter)
            .FirstOrDefaultAsync(a => a.Id == id);
        return Appointment == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null) return NotFound();
        var a = await _db.Appointments.FindAsync(id);
        if (a != null)
        {
            _db.Appointments.Remove(a);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}
