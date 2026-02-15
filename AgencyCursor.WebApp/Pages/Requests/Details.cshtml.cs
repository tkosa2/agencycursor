using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DetailsModel(AgencyDbContext db) => _db = db;

    public new Request? Request { get; set; }
    public IList<Appointment> Appointments { get; set; } = new List<Appointment>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Request = await _db.Requests
            .Include(r => r.Requestor)
            .Include(r => r.PreferredInterpreter)
            .Include(r => r.Appointments)
            .ThenInclude(a => a.Interpreter)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (Request == null) return NotFound();
        Appointments = Request.Appointments.OrderByDescending(a => a.ServiceDateTime).ToList();
        return Page();
    }
}
