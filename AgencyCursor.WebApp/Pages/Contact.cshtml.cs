using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Pages;

public class ContactModel : PageModel
{
    [BindProperty]
    public ContactFormModel ContactForm { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // In a real application, you would send an email or save to database here
        // For now, we'll just show a success message
        
        TempData["SuccessMessage"] = $"Thank you, {ContactForm.Name}! Your message has been received. We'll get back to you soon.";
        return RedirectToPage("/Contact");
    }
}

public class ContactFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(50)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;
}
