using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class Invoice
{
    public int Id { get; set; }

    [Required]
    public int RequestorId { get; set; }
    [ForeignKey(nameof(RequestorId))]
    public Requestor Requestor { get; set; } = null!;

    [Required]
    public int AppointmentId { get; set; }
    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    [Required]
    public int InterpreterId { get; set; }
    [ForeignKey(nameof(InterpreterId))]
    public Interpreter Interpreter { get; set; } = null!;

    [Display(Name = "Service Type"), StringLength(100)]
    public string? ServiceType { get; set; }

    [Display(Name = "Hours Worked")]
    public decimal HoursWorked { get; set; }

    [Display(Name = "Hourly Rate / Flat Fee")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Display(Name = "Total Cost")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    [Display(Name = "Payment Status"), StringLength(50)]
    public string PaymentStatus { get; set; } = "Pending";

    [Display(Name = "Invoice Number"), StringLength(50)]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Payment Method"), StringLength(100)]
    public string? PaymentMethod { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}
