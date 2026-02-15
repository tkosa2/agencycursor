using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requestors;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;

    public DetailsModel(AgencyDbContext db) => _db = db;

    public Requestor? Requestor { get; set; }
    public IList<Request> Requests { get; set; } = new List<Request>();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Requestor = await _db.Requestors
            .Include(r => r.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (Requestor == null) return NotFound();
        Requests = Requestor.Requests.OrderByDescending(r => r.ServiceDateTime).ToList();
        return Page();
    }
}
