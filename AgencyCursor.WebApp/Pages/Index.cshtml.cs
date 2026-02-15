using AgencyCursor.Data;
using AgencyCursor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages;

public class IndexModel : PageModel
{
    private readonly AgencyDbContext _db;

    public IndexModel(AgencyDbContext db) => _db = db;

    public int RequestorsCount { get; set; }
    public int InterpretersCount { get; set; }
    public int PendingRequestsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public int AssignedAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int PendingInvoicesCount { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
    public List<Request> Requests { get; set; } = new();

    public async Task OnGetAsync()
    {
        RequestorsCount = _db.Requestors.Count();
        InterpretersCount = _db.Interpreters.Count();
        PendingRequestsCount = _db.Requests.Count(r => r.Status == "Pending");
        PendingAppointmentsCount = _db.Appointments.Count(a => a.Status == "Pending");
        AssignedAppointmentsCount = _db.Appointments.Count(a => a.Status == "Confirmed" || a.Status == "Assigned" || a.Status == "In Progress");
        CompletedAppointmentsCount = _db.Appointments.Count(a => a.Status == "Completed");
        PendingInvoicesCount = _db.Invoices.Count(i => i.PaymentStatus == "Pending");
        
        // Load appointments for calendar (current month and next month)
        var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var endDate = startDate.AddMonths(2).AddDays(-1);
        Appointments = await _db.Appointments
            .Include(a => a.Request)
            .ThenInclude(r => r!.Requestor)
            .Include(a => a.Interpreter)
            .Where(a => a.ServiceDateTime >= startDate && a.ServiceDateTime <= endDate)
            .OrderBy(a => a.ServiceDateTime)
            .ToListAsync();
        
        // Load pending requests for calendar
        Requests = await _db.Requests
            .Include(r => r.Requestor)
            .Where(r => r.Status == "Pending" && r.ServiceDateTime >= startDate && r.ServiceDateTime <= endDate)
            .OrderBy(r => r.ServiceDateTime)
            .ToListAsync();
    }
}
