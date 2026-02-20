using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public Request Request { get; set; } = null!;

    // Keeping for backwards compatibility during migration, will be deprecated
    public int? InterpreterId { get; set; }
    [ForeignKey(nameof(InterpreterId))]
    public Interpreter? Interpreter { get; set; }

    [Display(Name = "Date and Time of Service")]
    public DateTime ServiceDateTime { get; set; }

    [StringLength(50)]
    public string? Location { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Service Details"), StringLength(500)]
    public string? ServiceDetails { get; set; }

    [Display(Name = "Duration (minutes)")]
    public int? DurationMinutes { get; set; }

    [Display(Name = "Additional Notes"), StringLength(2000)]
    public string? AdditionalNotes { get; set; }

    // Team of interpreters for this appointment
    public ICollection<AppointmentInterpreter> AppointmentInterpreters { get; set; } = new List<AppointmentInterpreter>();

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
