using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages.Interpreters;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;

    public CreateModel(AgencyDbContext db) => _db = db;

    [BindProperty]
    public Interpreter Interpreter { get; set; } = null!;

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        
        // Set as registered with agency by default
        Interpreter.IsRegisteredWithAgency = true;
        
        _db.Interpreters.Add(Interpreter);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index", new { registeredOnly = true });
    }
}
