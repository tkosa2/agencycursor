using AgencyCursor.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgencyCursor.Pages;

public class TestEmailLogModel : PageModel
{
    public List<TestEmailEntry> Entries { get; private set; } = new();

    public void OnGet(bool? clear)
    {
        if (clear == true)
        {
            TestEmailStore.Clear();
        }

        Entries = TestEmailStore.GetAll()
            .OrderByDescending(e => e.SentAt)
            .ToList();
    }
}
