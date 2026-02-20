using System.Collections.Concurrent;

namespace AgencyCursor.Services;

public class TestEmailEntry
{
    public DateTime SentAt { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
    public int? RequestId { get; set; }
    public int? InterpreterId { get; set; }
}

public static class TestEmailStore
{
    private const int MaxEntries = 200;
    private static readonly ConcurrentQueue<TestEmailEntry> Entries = new();

    public static void Record(TestEmailEntry entry)
    {
        entry.SentAt = DateTime.UtcNow;
        Entries.Enqueue(entry);
        while (Entries.Count > MaxEntries && Entries.TryDequeue(out _))
        {
        }
    }

    public static IReadOnlyList<TestEmailEntry> GetAll()
    {
        return Entries.ToArray();
    }

    public static void Clear()
    {
        while (Entries.TryDequeue(out _))
        {
        }
    }
}
