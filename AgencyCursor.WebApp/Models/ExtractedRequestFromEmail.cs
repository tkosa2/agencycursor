namespace AgencyCursor.Models;

/// <summary>
/// Data extracted from a request email (plain text).
/// </summary>
public class ExtractedRequestFromEmail
{
    // Requestor
    public string? RequestorName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    // Request
    public string? TypeOfService { get; set; }
    public string? Location { get; set; }
    public DateTime? ServiceDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? PreferredInterpreterName { get; set; }
    public string? Specialization { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? GenderPreference { get; set; }

    /// <summary>
    /// Raw notes combining specialization, gender preference, and additional info for Request.AdditionalNotes.
    /// </summary>
    public string? CombinedNotes => string.Join("\n",
        new[] { GenderPreference, Specialization, AdditionalNotes }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim()));
}
