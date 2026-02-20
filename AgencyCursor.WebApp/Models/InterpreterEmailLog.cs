namespace AgencyCursor.Models;

public class InterpreterEmailLog
{
    public int InterpreterEmailLogId { get; set; }
    public int RequestId { get; set; }
    public int InterpreterId { get; set; }
    public DateTime SentAt { get; set; }
    public string Status { get; set; } = "Success"; // "Success" or "Failed"
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public Request? Request { get; set; }
    public Interpreter? Interpreter { get; set; }
}
