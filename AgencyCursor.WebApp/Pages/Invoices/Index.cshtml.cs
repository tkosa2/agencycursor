using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Invoices;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db) => _db = db;

    public IList<Invoice> Invoices { get; set; } = new List<Invoice>();

    public void OnGet()
    {
        Invoices = _db.Invoices
            .Include(i => i.Requestor)
            .Include(i => i.Interpreter)
            .Include(i => i.Appointment)
            .OrderByDescending(i => i.Id)
            .ToList();
    }
}
