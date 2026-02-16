using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Models;

public class Interpreter
{
    public int Id { get; set; }

    [Required, Display(Name = "Interpreter Name"), StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Languages of Expertise"), StringLength(500)]
    public string? Languages { get; set; }

    [StringLength(1000)]
    public string? Availability { get; set; }

    [Phone, StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [Display(Name = "Address Line 1"), StringLength(200)]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address Line 2"), StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(50)]
    public string? State { get; set; }

    [Display(Name = "Zip Code"), StringLength(10)]
    public string? ZipCode { get; set; }

    [StringLength(200)]
    public string? Certification { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Display(Name = "Registered with Agency")]
    public bool IsRegisteredWithAgency { get; set; } = false;

    public ICollection<Request> PreferredForRequests { get; set; } = new List<Request>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
