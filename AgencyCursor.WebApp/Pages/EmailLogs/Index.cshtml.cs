using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.EmailLogs;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db)
    {
        _db = db;
    }

    public List<InterpreterEmailLog> EmailLogs { get; set; } = new();
    public string? FilterStatus { get; set; }
    public int FilterRequestId { get; set; }

    public async Task OnGetAsync(string? status, int? requestId)
    {
        FilterStatus = status;
        if (requestId.HasValue) FilterRequestId = requestId.Value;

        var query = _db.InterpreterEmailLogs
            .Include(e => e.Interpreter)
            .Include(e => e.Request)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(e => e.Status == status);
        }

        if (requestId.HasValue && requestId > 0)
        {
            query = query.Where(e => e.RequestId == requestId);
        }

        EmailLogs = await query.OrderByDescending(e => e.SentAt).ToListAsync();
    }
}
