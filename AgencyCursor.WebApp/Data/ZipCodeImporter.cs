using AgencyCursor.Models;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public static class ZipCodeImporter
{
    public static async Task ImportZipCodesAsync(AgencyDbContext db, string filePath)
    {
        // Check if zip codes already exist
        try
        {
            if (await db.ZipCodes.AnyAsync())
            {
                Console.WriteLine("Zip codes already exist in database. Skipping import.");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check existing zip codes: {ex.Message}");
            // Continue with import if check fails
        }

        // Ensure the ZipCodes table exists
        try
        {
            await db.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""ZipCodes"" (
                    ""Id"" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""CountryCode"" TEXT NOT NULL,
                    ""PostalCode"" TEXT NOT NULL,
                    ""PlaceName"" TEXT,
                    ""AdminName1"" TEXT,
                    ""AdminCode1"" TEXT,
                    ""AdminName2"" TEXT,
                    ""AdminCode2"" TEXT,
                    ""AdminName3"" TEXT,
                    ""AdminCode3"" TEXT,
                    ""Latitude"" REAL,
                    ""Longitude"" REAL,
                    ""Accuracy"" INTEGER
                );
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not ensure ZipCodes table exists: {ex.Message}");
        }

        var zipCodes = new List<ZipCode>();
        
        // Check if file exists and get info
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at {filePath}");
            return;
        }
        
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"File size: {fileInfo.Length} bytes");
        
        // Read the entire file content
        var fileContent = await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
        Console.WriteLine($"File content length: {fileContent.Length} characters");
        
        if (string.IsNullOrWhiteSpace(fileContent))
        {
            Console.WriteLine("Error: File appears to be empty or could not be read.");
            return;
        }
        
        // Split by newlines - handle both Windows (\r\n) and Unix (\n) line endings
        var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        Console.WriteLine($"Reading {lines.Length} lines from {filePath}...");

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split('\t');
            // Need at least 12 fields: country, postal, place, admin1 name, admin1 code, admin2 name, admin2 code, admin3 name, admin3 code, lat, lon, accuracy
            if (parts.Length < 12)
            {
                Console.WriteLine($"Warning: Skipping line with only {parts.Length} fields: {line.Substring(0, Math.Min(50, line.Length))}...");
                continue;
            }

            try
            {
                var zipCode = new ZipCode
                {
                    CountryCode = parts[0].Trim(),
                    PostalCode = parts[1].Trim(),
                    PlaceName = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2].Trim() : null,
                    AdminName1 = parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3]) ? parts[3].Trim() : null,
                    AdminCode1 = parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4].Trim() : null,
                    AdminName2 = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5].Trim() : null,
                    AdminCode2 = parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6].Trim() : null,
                    AdminName3 = parts.Length > 7 && !string.IsNullOrWhiteSpace(parts[7]) ? parts[7].Trim() : null,
                    AdminCode3 = parts.Length > 8 && !string.IsNullOrWhiteSpace(parts[8]) ? parts[8].Trim() : null,
                    Latitude = parts.Length > 9 && !string.IsNullOrWhiteSpace(parts[9]) && double.TryParse(parts[9].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : null,
                    Longitude = parts.Length > 10 && !string.IsNullOrWhiteSpace(parts[10]) && double.TryParse(parts[10].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon) ? lon : null,
                    Accuracy = parts.Length > 11 && !string.IsNullOrWhiteSpace(parts[11]) && int.TryParse(parts[11].Trim(), out var acc) ? acc : null
                };

                zipCodes.Add(zipCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing line: {line.Substring(0, Math.Min(50, line.Length))}... Error: {ex.Message}");
            }
        }

        Console.WriteLine($"Parsed {zipCodes.Count} zip codes. Saving to database...");

        // Batch insert for better performance
        const int batchSize = 1000;
        for (int i = 0; i < zipCodes.Count; i += batchSize)
        {
            var batch = zipCodes.Skip(i).Take(batchSize);
            await db.ZipCodes.AddRangeAsync(batch);
            await db.SaveChangesAsync();
            Console.WriteLine($"Imported {Math.Min(i + batchSize, zipCodes.Count)} of {zipCodes.Count} zip codes...");
        }

        Console.WriteLine($"Successfully imported {zipCodes.Count} zip codes into the database.");
    }
}
