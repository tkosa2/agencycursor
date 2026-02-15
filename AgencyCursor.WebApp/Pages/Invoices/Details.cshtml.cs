using AgencyCursor.Data;
using AgencyCursor.Models;
using AgencyCursor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Invoices;

public class DetailsModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly InvoicePdfService _pdfService;

    public DetailsModel(AgencyDbContext db, InvoicePdfService pdfService)
    {
        _db = db;
        _pdfService = pdfService;
    }

    public Invoice? Invoice { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();
        Invoice = await _db.Invoices
            .Include(i => i.Requestor)
            .Include(i => i.Interpreter)
            .Include(i => i.Appointment)
            .ThenInclude(a => a!.Request)
            .FirstOrDefaultAsync(m => m.Id == id);
        return Invoice == null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync(int? id, bool detailed = false)
    {
        if (id == null) return NotFound();
        
        var invoice = await _db.Invoices
            .Include(i => i.Requestor)
            .Include(i => i.Interpreter)
            .Include(i => i.Appointment)
            .ThenInclude(a => a!.Request)
            .FirstOrDefaultAsync(m => m.Id == id);
            
        if (invoice == null) return NotFound();

        var pdfBytes = _pdfService.GeneratePdf(invoice, detailed);
        var formatSuffix = detailed ? "_Detailed" : "_Standard";
        var fileName = $"Invoice_{invoice.InvoiceNumber ?? $"INV-{invoice.Id}"}{formatSuffix}_{DateTime.Now:yyyyMMdd}.pdf";
        
        return File(pdfBytes, "application/pdf", fileName);
    }
}
