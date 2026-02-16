using AgencyCursor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AgencyCursor.Data;

public static class InterpreterRegistrationService
{
    public static async Task<Interpreter?> ImportInterpreterFromRidAsync(
        AgencyDbContext db,
        string ridDbPath,
        Dictionary<string, object?> ridInterpreterData)
    {
        if (!File.Exists(ridDbPath))
        {
            return null;
        }

        try
        {
            using var connection = new SqliteConnection($"Data Source={ridDbPath}");
            await connection.OpenAsync();

            // Get table name
            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            var tables = new List<string>();
            using (var tableReader = await tableCommand.ExecuteReaderAsync())
            {
                while (await tableReader.ReadAsync())
                {
                    tables.Add(tableReader.GetString(0));
                }
            }

            string tableName = tables.FirstOrDefault(t =>
                t.ToLower().Contains("interpreter") ||
                t.ToLower().Contains("member") ||
                t.ToLower().Contains("rid")) ?? tables.FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(tableName))
            {
                return null;
            }

            // Get all columns
            using var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = $"PRAGMA table_info({tableName});";
            var columns = new List<string>();
            using (var pragmaReader = await pragmaCommand.ExecuteReaderAsync())
            {
                while (await pragmaReader.ReadAsync())
                {
                    columns.Add(pragmaReader.GetString(1));
                }
            }

            // Find key columns for matching
            var nameColumn = columns.FirstOrDefault(c =>
                c.ToLower() == "name" ||
                c.ToLower().Contains("fullname") ||
                (c.ToLower().Contains("name") && !c.ToLower().Contains("first") && !c.ToLower().Contains("last")));

            var firstNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("firstname") || c.ToLower() == "first");
            var lastNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lastname") || c.ToLower() == "last");
            var emailColumn = columns.FirstOrDefault(c => c.ToLower().Contains("email"));
            var phoneColumn = columns.FirstOrDefault(c => c.ToLower().Contains("phone") || c.ToLower().Contains("tel"));

            // Build WHERE clause to find the exact interpreter
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            // Try to match by name or email
            if (ridInterpreterData.ContainsKey("Name") && !string.IsNullOrWhiteSpace(ridInterpreterData["Name"]?.ToString()))
            {
                if (nameColumn != null)
                {
                    whereConditions.Add($"{nameColumn} = @name");
                    parameters["@name"] = ridInterpreterData["Name"];
                }
                else if (firstNameColumn != null && lastNameColumn != null)
                {
                    var nameParts = ridInterpreterData["Name"].ToString()!.Split(' ', 2);
                    if (nameParts.Length >= 1)
                    {
                        whereConditions.Add($"{firstNameColumn} = @firstName");
                        parameters["@firstName"] = nameParts[0];
                    }
                    if (nameParts.Length >= 2)
                    {
                        whereConditions.Add($"{lastNameColumn} = @lastName");
                        parameters["@lastName"] = nameParts[1];
                    }
                }
            }

            if (ridInterpreterData.ContainsKey("Email") && !string.IsNullOrWhiteSpace(ridInterpreterData["Email"]?.ToString()) && emailColumn != null)
            {
                whereConditions.Add($"{emailColumn} = @email");
                parameters["@email"] = ridInterpreterData["Email"];
            }

            if (!whereConditions.Any())
            {
                return null;
            }

            // Get all data for this interpreter
            var query = $"SELECT * FROM {tableName} WHERE {string.Join(" AND ", whereConditions)} LIMIT 1;";

            using var command = connection.CreateCommand();
            command.CommandText = query;
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            // Read all fields into a dictionary
            var allData = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                allData[columnName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            // Extract common fields
            string name = "";
            if (allData.ContainsKey(nameColumn ?? ""))
            {
                name = allData[nameColumn!]?.ToString()?.Trim() ?? "";
            }
            else if (allData.ContainsKey(firstNameColumn ?? "") || allData.ContainsKey(lastNameColumn ?? ""))
            {
                var firstName = allData.GetValueOrDefault(firstNameColumn ?? "")?.ToString()?.Trim() ?? "";
                var lastName = allData.GetValueOrDefault(lastNameColumn ?? "")?.ToString()?.Trim() ?? "";
                name = $"{firstName} {lastName}".Trim();
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            // Check if already exists in agency.db
            var emailValue = allData.GetValueOrDefault(emailColumn ?? "")?.ToString();
            var existing = await db.Interpreters
                .FirstOrDefaultAsync(i => i.Name == name || 
                    (!string.IsNullOrEmpty(i.Email) && !string.IsNullOrEmpty(emailValue) && i.Email == emailValue));

            if (existing != null)
            {
                // Update existing interpreter with RID data
                existing.Email = allData.GetValueOrDefault(emailColumn ?? "")?.ToString() ?? existing.Email;
                existing.Phone = allData.GetValueOrDefault(phoneColumn ?? "")?.ToString() ?? existing.Phone;
                existing.IsRegisteredWithAgency = true;
                
                // Store all RID data as JSON in Notes or create a separate field
                var ridDataJson = JsonSerializer.Serialize(allData);
                existing.Notes = $"RID Data: {ridDataJson}\n\n{existing.Notes}";
                
                await db.SaveChangesAsync();
                return existing;
            }

            // Create new interpreter
            var interpreter = new Interpreter
            {
                Name = name,
                Email = allData.GetValueOrDefault(emailColumn ?? "")?.ToString(),
                Phone = allData.GetValueOrDefault(phoneColumn ?? "")?.ToString(),
                Certification = allData.FirstOrDefault(kv => 
                    kv.Key.ToLower().Contains("cert") || 
                    kv.Key.ToLower().Contains("credential") || 
                    kv.Key.ToLower().Contains("rid")).Value?.ToString(),
                Languages = allData.FirstOrDefault(kv => 
                    kv.Key.ToLower().Contains("lang") || 
                    kv.Key.ToLower().Contains("language")).Value?.ToString() ?? "ASL",
                Availability = "Contact for availability",
                IsRegisteredWithAgency = true,
                Notes = $"Imported from RID database. Full data: {JsonSerializer.Serialize(allData)}"
            };

            db.Interpreters.Add(interpreter);
            await db.SaveChangesAsync();
            return interpreter;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing interpreter from RID: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Searches interpreters from the RID database and registers them in the agency database.
    /// </summary>
    public static async Task<RegistrationResult> SearchAndRegisterInterpretersAsync(
        AgencyDbContext db,
        string ridDbPath,
        int? limit = null,
        string? state = null,
        string? city = null)
    {
        var result = new RegistrationResult();
        
        if (!File.Exists(ridDbPath))
        {
            result.ErrorMessage = "RID database not found.";
            return result;
        }

        try
        {
            using var connection = new SqliteConnection($"Data Source={ridDbPath}");
            await connection.OpenAsync();

            // Get table name
            using var tableCommand = connection.CreateCommand();
            tableCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            var tables = new List<string>();
            using (var tableReader = await tableCommand.ExecuteReaderAsync())
            {
                while (await tableReader.ReadAsync())
                {
                    tables.Add(tableReader.GetString(0));
                }
            }

            string tableName = tables.FirstOrDefault(t =>
                t.ToLower().Contains("interpreter") ||
                t.ToLower().Contains("member") ||
                t.ToLower().Contains("rid")) ?? tables.FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(tableName))
            {
                result.ErrorMessage = "No suitable table found in RID database.";
                return result;
            }

            // Get all columns
            using var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = $"PRAGMA table_info({tableName});";
            var columns = new List<string>();
            using (var pragmaReader = await pragmaCommand.ExecuteReaderAsync())
            {
                while (await pragmaReader.ReadAsync())
                {
                    columns.Add(pragmaReader.GetString(1));
                }
            }

            // Find key columns
            var nameColumn = columns.FirstOrDefault(c =>
                c.ToLower() == "name" ||
                c.ToLower().Contains("fullname") ||
                (c.ToLower().Contains("name") && !c.ToLower().Contains("first") && !c.ToLower().Contains("last")));

            var firstNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("firstname") || c.ToLower() == "first");
            var lastNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lastname") || c.ToLower() == "last");
            var emailColumn = columns.FirstOrDefault(c => c.ToLower().Contains("email"));
            var phoneColumn = columns.FirstOrDefault(c => c.ToLower().Contains("phone") || c.ToLower().Contains("tel"));
            // Find state column - try various common names
            var stateColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "state" ||
                c.ToLower().Contains("state") || 
                c.ToLower().Contains("province") ||
                c.ToLower() == "st" ||
                c.ToLower() == "region");
            var cityColumn = columns.FirstOrDefault(c => c.ToLower().Contains("city"));

            // Build WHERE clause for filtering
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            if (!string.IsNullOrWhiteSpace(state) && stateColumn != null)
            {
                var stateValue = state.Trim();
                var stateLower = stateValue.ToLower();
                
                // Map common state abbreviations to full names
                var stateAbbreviationMap = new Dictionary<string, string>
                {
                    { "wa", "washington" },
                    { "ia", "iowa" },
                    { "hi", "hawaii" },
                    { "de", "delaware" },
                    { "ca", "california" },
                    { "ny", "new york" },
                    { "tx", "texas" },
                    { "fl", "florida" },
                    { "il", "illinois" },
                    { "pa", "pennsylvania" },
                    { "oh", "ohio" },
                    { "ga", "georgia" },
                    { "nc", "north carolina" },
                    { "mi", "michigan" },
                    { "nj", "new jersey" },
                    { "va", "virginia" },
                    { "az", "arizona" },
                    { "ma", "massachusetts" },
                    { "tn", "tennessee" },
                    { "in", "indiana" },
                    { "mo", "missouri" },
                    { "md", "maryland" },
                    { "wi", "wisconsin" },
                    { "co", "colorado" },
                    { "mn", "minnesota" },
                    { "sc", "south carolina" },
                    { "al", "alabama" },
                    { "la", "louisiana" },
                    { "ky", "kentucky" },
                    { "or", "oregon" },
                    { "ok", "oklahoma" },
                    { "ct", "connecticut" },
                    { "ut", "utah" },
                    { "ar", "arkansas" },
                    { "nv", "nevada" },
                    { "ms", "mississippi" },
                    { "ks", "kansas" },
                    { "nm", "new mexico" },
                    { "ne", "nebraska" },
                    { "wv", "west virginia" },
                    { "id", "idaho" },
                    { "nh", "new hampshire" },
                    { "me", "maine" },
                    { "ri", "rhode island" },
                    { "mt", "montana" },
                    { "sd", "south dakota" },
                    { "nd", "north dakota" },
                    { "ak", "alaska" },
                    { "vt", "vermont" },
                    { "wy", "wyoming" }
                };
                
                // Build condition to match either abbreviation or full name
                // Use exact match to avoid matching states that contain the search term
                // e.g., searching "WA" should not match "IOWA", "HAWAII", or "DELAWARE"
                if (stateValue.Length == 2 && stateAbbreviationMap.ContainsKey(stateLower))
                {
                    // Search for both abbreviation (exact match) and full name (starts with, to handle "Washington State")
                    var fullName = stateAbbreviationMap[stateLower];
                    // For abbreviation: exact match only (prevents matching "IOWA", "HAWAII", "DELAWARE")
                    // For full name: starts with match (handles "Washington" and "Washington State")
                    // Using LIKE with fullName% is safe because "washington" won't match "iowa", "hawaii", or "delaware"
                    whereConditions.Add($"(LOWER(TRIM({stateColumn})) = LOWER(@stateAbbr) OR LOWER(TRIM({stateColumn})) LIKE LOWER(@stateFullLike))");
                    parameters["@stateAbbr"] = stateValue;
                    parameters["@stateFullLike"] = $"{fullName}%"; // Matches "Washington" or "Washington State"
                }
                else
                {
                    // For longer inputs, assume it's a full state name
                    // Use starts-with match to handle variations like "Washington State"
                    whereConditions.Add($"LOWER(TRIM({stateColumn})) LIKE LOWER(@stateLike)");
                    parameters["@stateLike"] = $"{stateValue.ToLower()}%";
                }
            }

            if (!string.IsNullOrWhiteSpace(city) && cityColumn != null)
            {
                whereConditions.Add($"{cityColumn} LIKE @city");
                parameters["@city"] = $"%{city}%";
            }

            // Build SELECT query
            var query = $"SELECT * FROM {tableName}";
            if (whereConditions.Any())
            {
                query += " WHERE " + string.Join(" AND ", whereConditions);
            }
            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }
            else
            {
                query += " LIMIT 10000"; // Default limit to prevent memory issues
            }

            using var command = connection.CreateCommand();
            command.CommandText = query;
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            using var reader = await command.ExecuteReaderAsync();
            
            const int batchSize = 50;
            var batch = new List<Dictionary<string, object?>>();

            while (await reader.ReadAsync())
            {
                // Read all fields into a dictionary
                var allData = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    allData[columnName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                batch.Add(allData);

                if (batch.Count >= batchSize)
                {
                    await ProcessBatchAsync(db, batch, nameColumn, firstNameColumn, lastNameColumn, emailColumn, phoneColumn, result);
                    batch.Clear();
                }
            }

            // Process remaining batch
            if (batch.Any())
            {
                await ProcessBatchAsync(db, batch, nameColumn, firstNameColumn, lastNameColumn, emailColumn, phoneColumn, result);
            }

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error searching and registering interpreters: {ex.Message}";
            Console.WriteLine(result.ErrorMessage);
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return result;
        }
    }

    private static async Task ProcessBatchAsync(
        AgencyDbContext db,
        List<Dictionary<string, object?>> batch,
        string? nameColumn,
        string? firstNameColumn,
        string? lastNameColumn,
        string? emailColumn,
        string? phoneColumn,
        RegistrationResult result)
    {
        foreach (var allData in batch)
        {
            try
            {
                // Extract name
                string name = "";
                if (!string.IsNullOrEmpty(nameColumn) && allData.ContainsKey(nameColumn))
                {
                    name = allData[nameColumn]?.ToString()?.Trim() ?? "";
                }
                else if ((!string.IsNullOrEmpty(firstNameColumn) && allData.ContainsKey(firstNameColumn)) ||
                         (!string.IsNullOrEmpty(lastNameColumn) && allData.ContainsKey(lastNameColumn)))
                {
                    var firstName = allData.GetValueOrDefault(firstNameColumn ?? "")?.ToString()?.Trim() ?? "";
                    var lastName = allData.GetValueOrDefault(lastNameColumn ?? "")?.ToString()?.Trim() ?? "";
                    name = $"{firstName} {lastName}".Trim();
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Skipped++;
                    continue;
                }

                // Check if already exists in agency.db
                var emailValue = allData.GetValueOrDefault(emailColumn ?? "")?.ToString();
                var existing = await db.Interpreters
                    .FirstOrDefaultAsync(i => i.Name == name ||
                        (!string.IsNullOrEmpty(i.Email) && !string.IsNullOrEmpty(emailValue) && i.Email == emailValue));

                if (existing != null)
                {
                    // Update existing interpreter with RID data and mark as registered
                    if (!existing.IsRegisteredWithAgency)
                    {
                        existing.Email = allData.GetValueOrDefault(emailColumn ?? "")?.ToString() ?? existing.Email;
                        existing.Phone = allData.GetValueOrDefault(phoneColumn ?? "")?.ToString() ?? existing.Phone;
                        existing.IsRegisteredWithAgency = true;
                        
                        var ridDataJson = JsonSerializer.Serialize(allData);
                        existing.Notes = $"RID Data: {ridDataJson}\n\n{existing.Notes ?? ""}";
                        
                        result.Registered++;
                    }
                    else
                    {
                        result.AlreadyRegistered++;
                    }
                }
                else
                {
                    // Create new interpreter
                    var interpreter = new Interpreter
                    {
                        Name = name,
                        Email = allData.GetValueOrDefault(emailColumn ?? "")?.ToString(),
                        Phone = allData.GetValueOrDefault(phoneColumn ?? "")?.ToString(),
                        Certification = allData.FirstOrDefault(kv =>
                            kv.Key.ToLower().Contains("cert") ||
                            kv.Key.ToLower().Contains("credential") ||
                            kv.Key.ToLower().Contains("rid")).Value?.ToString(),
                        Languages = allData.FirstOrDefault(kv =>
                            kv.Key.ToLower().Contains("lang") ||
                            kv.Key.ToLower().Contains("language")).Value?.ToString() ?? "ASL",
                        Availability = "Contact for availability",
                        IsRegisteredWithAgency = true,
                        Notes = $"Imported from RID database. Full data: {JsonSerializer.Serialize(allData)}"
                    };

                    db.Interpreters.Add(interpreter);
                    result.Created++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing interpreter: {ex.Message}");
                result.Errors++;
            }
        }

        await db.SaveChangesAsync();
    }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public int Created { get; set; }
    public int Registered { get; set; }
    public int AlreadyRegistered { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public string? ErrorMessage { get; set; }

    public int TotalProcessed => Created + Registered + AlreadyRegistered + Skipped + Errors;
}
