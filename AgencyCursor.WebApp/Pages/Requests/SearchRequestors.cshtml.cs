using AgencyCursor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class SearchRequestorsModel : PageModel
{
    private readonly AgencyDbContext _db;

    public SearchRequestorsModel(AgencyDbContext db) => _db = db;

    public async Task<IActionResult> OnGetAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
        {
            return new JsonResult(new List<object>());
        }

        var requestors = await _db.Requestors
            .Where(r => r.Name.Contains(term) || 
                       (r.Email != null && r.Email.Contains(term)) ||
                       (r.Phone != null && r.Phone.Contains(term)))
            .OrderBy(r => r.Name)
            .Take(10)
            .Select(r => new
            {
                id = r.Id,
                name = r.Name,
                email = r.Email ?? "",
                phone = r.Phone ?? ""
            })
            .ToListAsync();

        return new JsonResult(requestors);
    }
}
