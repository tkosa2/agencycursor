using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class AppointmentInterpreter
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }
    [ForeignKey(nameof(AppointmentId))]
    public Appointment Appointment { get; set; } = null!;

    [Required]
    public int InterpreterId { get; set; }
    [ForeignKey(nameof(InterpreterId))]
    public Interpreter Interpreter { get; set; } = null!;
}
