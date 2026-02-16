using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages;

public class ZipCodesModel : PageModel
{
    private readonly AgencyDbContext _db;

    public ZipCodesModel(AgencyDbContext db) => _db = db;

    public List<ZipCode> TopZipCodes { get; set; } = new();
    public List<string> DistinctStates { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Get top 10 zipcodes
        TopZipCodes = await _db.ZipCodes
            .Take(10)
            .ToListAsync();

        // Get distinct states (AdminCode1 values)
        DistinctStates = await _db.ZipCodes
            .Where(z => !string.IsNullOrEmpty(z.AdminCode1))
            .Select(z => z.AdminCode1)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }
}
