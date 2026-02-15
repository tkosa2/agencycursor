using System.Globalization;
using AgencyCursor.Models;

namespace AgencyCursor.Services;

/// <summary>
/// Parses plain-text request emails and extracts requestor + request details.
/// </summary>
public static class RequestEmailParser
{
    /// <summary>
    /// Tries to extract request and requestor information from a plain text email.
    /// </summary>
    public static ExtractedRequestFromEmail Parse(string emailPlainText)
    {
        if (string.IsNullOrWhiteSpace(emailPlainText))
            return new ExtractedRequestFromEmail();

        var lines = emailPlainText
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(l => l.Trim())
            .ToList();

        string? GetValueAfterLabel(string label, bool useContains = false)
        {
            var idx = useContains
                ? lines.FindIndex(l => l.Contains(label, StringComparison.OrdinalIgnoreCase))
                : lines.FindIndex(l => l.StartsWith(label, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return null;
            var line = lines[idx];
            var colon = line.IndexOf(':');
            if (colon >= 0 && colon < line.Length - 1)
            {
                var rest = line[(colon + 1)..].Trim();
                if (!string.IsNullOrEmpty(rest)) return rest;
            }
            if (idx + 1 < lines.Count && !string.IsNullOrWhiteSpace(lines[idx + 1]) && !lines[idx + 1].Contains(':'))
                return lines[idx + 1];
            return null;
        }

        string? GetMultiLineAfterLabel(string label, int maxLines = 5)
        {
            var idx = lines.FindIndex(l => l.Contains(label, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return null;
            var parts = new List<string>();
            for (var i = idx + 1; i < lines.Count && parts.Count < maxLines; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) break;
                if (line.Contains(':') && !line.StartsWith(" ", StringComparison.Ordinal)) break;
                parts.Add(line);
            }
            return parts.Count > 0 ? string.Join(" ", parts).Trim() : null;
        }

        string? clientName = GetValueAfterLabel("Client Name:") ?? GetValueAfterLabel("Requestor Name:");
        string? phone = GetValueAfterLabel("Phone Number:") ?? GetValueAfterLabel("Phone:");
        string? email = GetValueAfterLabel("Email Address:") ?? GetValueAfterLabel("Email:");
        string? dateStr = GetValueAfterLabel("Date of Appointment:", true) ?? GetValueAfterLabel("Date:");
        string? startTime = GetValueAfterLabel("Start Time:", true);
        string? endTime = GetValueAfterLabel("End Time:", true);
        string? typeOfAppointment = GetValueAfterLabel("Type of Appointment:", true) ?? GetValueAfterLabel("Type of Service:");
        string? mode = GetValueAfterLabel("Mode of Interpretation:", true) ?? GetValueAfterLabel("Location:");
        string? address = GetValueAfterLabel("Address:", true) ?? GetValueAfterLabel("In-Person Appointment Location:", true);
        string? preferredInterpreter = GetValueAfterLabel("Preferred Interpreter", true);
        string? genderPref = GetValueAfterLabel("Gender Preference:", true);
        string? specialization = GetMultiLineAfterLabel("Specialization:", 10);
        string? additionalNotes = GetMultiLineAfterLabel("Additional Information/Notes:", 15) ?? GetMultiLineAfterLabel("Additional Notes:", 10);

        if (string.IsNullOrEmpty(address))
        {
            var addrIdx = lines.FindIndex(l => l.Contains("Address:", StringComparison.OrdinalIgnoreCase));
            if (addrIdx >= 0 && addrIdx + 1 < lines.Count)
                address = lines[addrIdx + 1];
        }

        DateTime? serviceDate = null;
        if (!string.IsNullOrWhiteSpace(dateStr))
            serviceDate = ParseDate(dateStr);

        var serviceDateTime = serviceDate ?? DateTime.Today;
        if (!string.IsNullOrWhiteSpace(startTime) && ParseTime(startTime) is { } start)
            serviceDateTime = serviceDate!.Value.Date + start;

        return new ExtractedRequestFromEmail
        {
            RequestorName = clientName,
            Phone = phone,
            Email = email,
            Address = address,
            TypeOfService = typeOfAppointment,
            Location = NormalizeLocation(mode),
            ServiceDate = serviceDate,
            StartTime = startTime,
            EndTime = endTime,
            PreferredInterpreterName = preferredInterpreter,
            Specialization = specialization,
            AdditionalNotes = additionalNotes,
            GenderPreference = genderPref
        };
    }

    private static string? NormalizeLocation(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return null;
        if (mode.Contains("Virtual", StringComparison.OrdinalIgnoreCase)) return "Virtual";
        if (mode.Contains("In-Person", StringComparison.OrdinalIgnoreCase) || mode.Contains("In Person", StringComparison.OrdinalIgnoreCase)) return "In-Person";
        return mode.Trim();
    }

    private static DateTime? ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        dateStr = dateStr.Trim();
        if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParse(dateStr, new CultureInfo("en-US"), DateTimeStyles.None, out d))
            return d;
        return null;
    }

    private static TimeSpan? ParseTime(string timeStr)
    {
        if (string.IsNullOrWhiteSpace(timeStr)) return null;
        timeStr = timeStr.Trim();
        if (TimeSpan.TryParse(timeStr, out var t)) return t;
        if (DateTime.TryParseExact(timeStr, new[] { "h:mm tt", "h:m tt", "hh:mm tt", "H:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.TimeOfDay;
        if (DateTime.TryParseExact(timeStr, new[] { "h:mm tt", "h:m tt", "hh:mm tt", "H:mm" }, new CultureInfo("en-US"), DateTimeStyles.None, out dt))
            return dt.TimeOfDay;
        return null;
    }

    /// <summary>
    /// Builds a DateTime for the request from extracted date + start time.
    /// </summary>
    public static DateTime GetServiceDateTime(ExtractedRequestFromEmail extracted)
    {
        var date = extracted.ServiceDate ?? DateTime.Today;
        if (!string.IsNullOrWhiteSpace(extracted.StartTime) && ParseTime(extracted.StartTime) is { } start)
            return date.Date + start;
        return date.Date.AddHours(9);
    }
}
