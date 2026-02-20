using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgencyCursor.Models;

public class InterpreterResponse
{
    public int Id { get; set; }

    [Required]
    public int RequestId { get; set; }
    [ForeignKey(nameof(RequestId))]
    public Request Request { get; set; } = null!;

    [Required]
    public int InterpreterId { get; set; }
    [ForeignKey(nameof(InterpreterId))]
    public Interpreter Interpreter { get; set; } = null!;

    [Required, StringLength(20)]
    public string Status { get; set; } = string.Empty; // "Yes", "No", "Maybe"

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? ResponseToken { get; set; } // One-time or secure token for email link
}
