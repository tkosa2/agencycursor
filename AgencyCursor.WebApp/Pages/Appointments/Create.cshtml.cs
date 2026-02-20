using AgencyCursor.Data;
using AgencyCursor.Models;
using AgencyCursor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Pages.Appointments;

public class CreateModel : PageModel
{
    private readonly AgencyDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CreateModel(AgencyDbContext db, IEmailService emailService, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _db = db;
        _emailService = emailService;
        _configuration = configuration;
        _environment = environment;
    }

    [BindProperty]
    public Appointment Appointment { get; set; } = null!;

    public new Request? Request { get; set; }
    public DateTime? RequestEndDateTime { get; set; }
    public SelectList InterpreterList { get; set; } = null!;
    public SelectList? RequestList { get; set; }

    public async Task<IActionResult> OnGetAsync(int? requestId)
    {
        // Only show registered interpreters
        var interpreters = await _db.Interpreters
            .Where(i => i.IsRegisteredWithAgency)
            .OrderBy(i => i.Name)
            .ToListAsync();
        InterpreterList = new SelectList(interpreters, "Id", "Name");
        var requests = await _db.Requests.Include(r => r.Requestor).OrderByDescending(r => r.ServiceDateTime).ToListAsync();
        RequestList = new SelectList(requests.Select(r => new { r.Id, Display = $"#{r.Id} - {r.Requestor?.Name} - {r.ServiceDateTime:g}" }), "Id", "Display");
        if (requestId.HasValue)
        {
            Request = await _db.Requests.Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == requestId);
            if (Request == null) return NotFound();
            RequestEndDateTime = Request.EndDateTime;
            Appointment = new Appointment
            {
                RequestId = Request.Id,
                ServiceDateTime = Request.ServiceDateTime,
                Location = Request.Location ?? "",
                ServiceDetails = Request.TypeOfService,
                Status = "Pending",
                ClientEmployeeName = Request.ConsumerNames ?? ""
            };
            if (Request.PreferredInterpreterId.HasValue)
                Appointment.InterpreterId = Request.PreferredInterpreterId.Value;
        }
        else
        {
            Appointment = new Appointment
            {
                ServiceDateTime = DateTime.Now.AddDays(1).Date.AddHours(9), // Default to tomorrow at 9 AM
                Status = "Pending"
            };
            if (requests.Count > 0)
                Appointment.RequestId = requests[0].Id;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Clear ModelState errors for our custom validation fields to avoid conflicts
        ModelState.Remove("Appointment.InterpreterId");
        ModelState.Remove("Appointment.RequestId");
        ModelState.Remove("Appointment.ServiceDateTime");

        // Validate required fields explicitly
        bool hasErrors = false;

        // Check InterpreterId - must be greater than 0
        if (Appointment.InterpreterId <= 0)
        {
            ModelState.AddModelError("Appointment.InterpreterId", "Please select an interpreter.");
            hasErrors = true;
        }
        else
        {
            // Verify interpreter exists and is registered
            var interpreterExists = await _db.Interpreters.AnyAsync(i => i.Id == Appointment.InterpreterId && i.IsRegisteredWithAgency);
            if (!interpreterExists)
            {
                ModelState.AddModelError("Appointment.InterpreterId", "Selected interpreter is not registered with the agency.");
                hasErrors = true;
            }
        }

        // RequestId is optional - if not provided, we'll create a request with admin requestor
        // If provided, verify it exists
        if (Appointment.RequestId > 0)
        {
            var requestExists = await _db.Requests.AnyAsync(r => r.Id == Appointment.RequestId);
            if (!requestExists)
            {
                ModelState.AddModelError("Appointment.RequestId", "Selected request does not exist.");
                hasErrors = true;
            }
        }

        // Validate ServiceDateTime - check if it's unset (default/min value or year 1)
        var minValidDate = new DateTime(1900, 1, 1);
        if (Appointment.ServiceDateTime < minValidDate)
        {
            ModelState.AddModelError("Appointment.ServiceDateTime", "Please select a date and time.");
            hasErrors = true;
        }

        if (hasErrors)
        {
            // Reload dropdowns
            await ReloadDropdownsAsync();
            return Page();
        }

        // If we get here, validation passed - proceed with save

        try
        {
            Request? request = null;
            
            // If no request is selected, create one with admin requestor
            if (Appointment.RequestId <= 0)
            {
                // Get or create admin requestor
                var adminRequestor = await _db.Requestors
                    .FirstOrDefaultAsync(r => r.Name == "Admin");
                
                if (adminRequestor == null)
                {
                    adminRequestor = new Requestor
                    {
                        Name = "Admin",
                        Email = "admin@agency.example.com",
                        Phone = "+1 (555) 000-0000",
                        Address = "Administrative Office",
                        Notes = "System requestor for admin-created appointments"
                    };
                    _db.Requestors.Add(adminRequestor);
                    await _db.SaveChangesAsync();
                }

                // Create a new request with admin requestor
                request = new Request
                {
                    RequestorId = adminRequestor.Id,
                    ServiceDateTime = Appointment.ServiceDateTime,
                    Location = Appointment.Location,
                    TypeOfService = Appointment.ServiceDetails ?? "Admin Created",
                    Status = "Assigned"
                };
                _db.Requests.Add(request);
                await _db.SaveChangesAsync();
                
                Appointment.RequestId = request.Id;
            }
            else
            {
                // Use existing request
                request = await _db.Requests.FindAsync(Appointment.RequestId);
                if (request != null)
                {
                    request.Status = "Assigned";
                }
            }

            _db.Appointments.Add(Appointment);
            await _db.SaveChangesAsync();

            // Send confirmation email to requestor
            try
            {
                // Reload appointment with full navigation properties
                var savedAppointment = await _db.Appointments
                    .Include(a => a.Interpreter)
                    .Include(a => a.Request)
                    .ThenInclude(r => r.Requestor)
                    .FirstOrDefaultAsync(a => a.Id == Appointment.Id);

                if (savedAppointment?.Request?.Requestor != null)
                {
                    var requestor = savedAppointment.Request.Requestor;
                    var recipientEmail = _environment.IsDevelopment() 
                        ? (_configuration["SmtpSettings:TestEmailAddress"] ?? "tkosa3@gmail.com")
                        : requestor.Email;

                    if (!string.IsNullOrEmpty(recipientEmail))
                    {
                        var confirmationEmail = BuildAppointmentConfirmationEmail(savedAppointment);
                        await _emailService.SendEmailAsync(
                            recipientEmail,
                            $"Appointment Confirmed - Interpreter Booked for {savedAppointment.ServiceDateTime:d}",
                            confirmationEmail);
                    }
                }
            }
            catch (Exception emailEx)
            {
                // Log but don't fail - email is non-critical
                Console.WriteLine($"Failed to send appointment confirmation email: {emailEx.Message}");
            }

            TempData["SuccessMessage"] = "Appointment created successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving appointment: {ex.Message}");
            await ReloadDropdownsAsync();
            return Page();
        }
    }

    private async Task ReloadDropdownsAsync()
    {
        var interpreters = await _db.Interpreters
            .Where(i => i.IsRegisteredWithAgency)
            .OrderBy(i => i.Name)
            .ToListAsync();
        InterpreterList = new SelectList(interpreters, "Id", "Name", Appointment.InterpreterId);
        var requests = await _db.Requests.Include(r => r.Requestor).OrderByDescending(r => r.ServiceDateTime).ToListAsync();
        RequestList = new SelectList(requests.Select(r => new { r.Id, Display = $"#{r.Id} - {r.Requestor?.Name} - {r.ServiceDateTime:g}" }), "Id", "Display");
        Request = await _db.Requests.Include(r => r.Requestor).FirstOrDefaultAsync(r => r.Id == Appointment.RequestId);
    }

    private string BuildAppointmentConfirmationEmail(Appointment appointment)
    {
        var requestor = appointment.Request?.Requestor;
        var interpreter = appointment.Interpreter;
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #0D6EFD; color: white; padding: 20px; border-radius: 5px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; border-radius: 5px; margin-top: 20px; }}
        .section {{ margin: 15px 0; }}
        .label {{ font-weight: bold; color: #0D6EFD; }}
        .badge {{ display: inline-block; padding: 5px 10px; background-color: #28A745; color: white; border-radius: 3px; }}
        .details {{ background-color: white; padding: 15px; border-left: 4px solid #0D6EFD; margin: 10px 0; }}
        .footer {{ text-align: center; margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Appointment Confirmed!</h1>
            <p>Your interpreter has been successfully booked</p>
        </div>

        <div class=""content"">
            <p>Dear {requestor?.Name},</p>

            <p>Your interpretation appointment has been confirmed and scheduled. Here are the details:</p>

            <div class=""details"">
                <div class=""section"">
                    <span class=""label"">Appointment Status:</span> <span class=""badge"">Confirmed & Booked</span>
                </div>
                
                <div class=""section"">
                    <span class=""label"">Date & Time:</span> {appointment.ServiceDateTime:dddd, MMMM d, yyyy} at {appointment.ServiceDateTime:h:mm tt}
                </div>

                <div class=""section"">
                    <span class=""label"">Interpreter:</span> {interpreter?.Name}
                </div>

                <div class=""section"">
                    <span class=""label"">Contact:</span> {interpreter?.Email} | {interpreter?.Phone}
                </div>

                <div class=""section"">
                    <span class=""label"">Service Type:</span> {appointment.ServiceDetails ?? "General"}
                </div>

                <div class=""section"">
                    <span class=""label"">Location:</span> {appointment.Location ?? "Virtual/Remote"}
                </div>

                @if (!string.IsNullOrEmpty(appointment.AdditionalNotes))
                {{
                    <div class=""section"">
                        <span class=""label"">Additional Notes:</span> {appointment.AdditionalNotes}
                    </div>
                }}
            </div>

            <p><strong>Next Steps:</strong></p>
            <ul>
                <li>The assigned interpreter will contact you to confirm any final details</li>
                <li>Please ensure you have any necessary materials or setup ready</li>
                <li>If you need to make changes, please contact us as soon as possible</li>
            </ul>

            <p>Thank you for choosing our interpretation services!</p>
        </div>

        <div class=""footer"">
            <p>This is an automated confirmation email. Please do not reply to this email.</p>
            <p>For questions or to reschedule, contact the agency directly.</p>
        </div>
    </div>
</body>
</html>";
    }
}
