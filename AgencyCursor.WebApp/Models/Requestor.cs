using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Models;

public class Requestor
{
    public int Id { get; set; }

    [Required, Display(Name = "Requestor Name"), StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "First Name"), StringLength(100)]
    public string? FirstName { get; set; }

    [Display(Name = "Last Name"), StringLength(100)]
    public string? LastName { get; set; }

    [Phone, StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(200)]
    public string? Address2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(2)]
    public string? State { get; set; }

    [StringLength(10)]
    public string? ZipCode { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
