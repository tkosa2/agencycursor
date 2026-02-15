using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Requestors;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Requestor Requestor { get; set; } = null!;

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        _db.Requestors.Add(Requestor);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
