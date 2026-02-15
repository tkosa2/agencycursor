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
}
