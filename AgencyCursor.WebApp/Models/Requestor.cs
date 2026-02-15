using System.ComponentModel.DataAnnotations;

namespace AgencyCursor.Models;

public class Requestor
{
    public int Id { get; set; }

    [Required, Display(Name = "Requestor Name"), StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Phone, StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress, StringLength(200)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
