using AgencyCursor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Requests;

public class SearchZipCodesModel : PageModel
{
    private readonly AgencyDbContext _db;

    public SearchZipCodesModel(AgencyDbContext db) => _db = db;

    public async Task<IActionResult> OnGetAsync(string zip)
    {
        if (string.IsNullOrWhiteSpace(zip))
        {
            return new JsonResult(null);
        }

        // Remove any non-numeric characters (handle ZIP+4 format)
        var zipCode = new string(zip.Where(char.IsDigit).Take(5).ToArray());

        if (zipCode.Length < 5)
        {
            return new JsonResult(null);
        }

        var zipData = await _db.ZipCodes
            .Where(z => z.PostalCode == zipCode)
            .OrderByDescending(z => z.Accuracy ?? 0) // Prefer higher accuracy
            .FirstOrDefaultAsync();

        if (zipData == null)
        {
            return new JsonResult(null);
        }

        return new JsonResult(new
        {
            city = zipData.PlaceName ?? "",
            state = zipData.AdminCode1 ?? zipData.AdminName1 ?? "",
            stateFull = zipData.AdminName1 ?? "",
            county = zipData.AdminName2 ?? "",
            latitude = zipData.Latitude,
            longitude = zipData.Longitude
        });
    }
}
