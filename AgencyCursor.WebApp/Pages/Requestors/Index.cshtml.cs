using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Requestors;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db) => _db = db;

    public IList<Requestor> Requestors { get; set; } = new List<Requestor>();

    public void OnGet()
    {
        Requestors = _db.Requestors.OrderBy(r => r.Name).ToList();
    }
}
