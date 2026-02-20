using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Interpreters;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DetailsModel(AgencyDbContext db) => _db = db;

    public Interpreter? Interpreter { get; set; }
    public IList<Appointment> Appointments { get; set; } = new List<Appointment>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Interpreter = await _db.Interpreters
            .Include(i => i.AppointmentInterpreters)
            .ThenInclude(ai => ai.Appointment)
            .ThenInclude(a => a.Request)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (Interpreter == null) return NotFound();
        Appointments = Interpreter.AppointmentInterpreters
            .Select(ai => ai.Appointment)
            .OrderByDescending(a => a.ServiceDateTime)
            .ToList();
        return Page();
    }
}
