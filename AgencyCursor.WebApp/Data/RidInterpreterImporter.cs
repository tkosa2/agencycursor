using AgencyCursor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public static class RidInterpreterImporter
{
    public static async Task ImportInterpretersAsync(AgencyDbContext db, string ridDbPath)
    {
        if (!File.Exists(ridDbPath))
        {
            Console.WriteLine($"RID interpreters database not found at {ridDbPath}");
            return;
        }

        // Check if interpreters already exist
        if (await db.Interpreters.AnyAsync())
        {
            Console.WriteLine("Interpreters already exist in database. Skipping import.");
            return;
        }

        var interpreters = new List<Interpreter>();
        var connectionString = $"Data Source={ridDbPath}";

        try
        {
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            // First, let's see what tables exist
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

            Console.WriteLine($"Found tables: {string.Join(", ", tables)}");

            // Try common table names
            string tableName = tables.FirstOrDefault(t => 
                t.ToLower().Contains("interpreter") || 
                t.ToLower().Contains("member") ||
                t.ToLower().Contains("rid")) ?? tables.FirstOrDefault() ?? "";

            if (string.IsNullOrEmpty(tableName))
            {
                Console.WriteLine("No suitable table found in RID database.");
                return;
            }

            Console.WriteLine($"Using table: {tableName}");

            // Get column names
            using var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = $"PRAGMA table_info({tableName});";
            var columns = new List<string>();
            using (var pragmaReader = await pragmaCommand.ExecuteReaderAsync())
            {
                while (await pragmaReader.ReadAsync())
                {
                    columns.Add(pragmaReader.GetString(1)); // Column name is at index 1
                }
            }

            Console.WriteLine($"Found columns: {string.Join(", ", columns)}");

            // Build query based on available columns
            var nameColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "name" || 
                c.ToLower().Contains("fullname") ||
                (c.ToLower().Contains("name") && !c.ToLower().Contains("first") && !c.ToLower().Contains("last")));

            var firstNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("firstname") || c.ToLower() == "first");
            var lastNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lastname") || c.ToLower() == "last");
            var emailColumn = columns.FirstOrDefault(c => c.ToLower().Contains("email"));
            var phoneColumn = columns.FirstOrDefault(c => c.ToLower().Contains("phone") || c.ToLower().Contains("tel"));
            var certColumn = columns.FirstOrDefault(c => c.ToLower().Contains("cert") || c.ToLower().Contains("credential") || c.ToLower().Contains("rid"));
            var langColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lang") || c.ToLower().Contains("language"));

            // Build SELECT with name handling
            var selectColumns = new List<string>();
            if (nameColumn != null)
            {
                selectColumns.Add(nameColumn);
            }
            else if (firstNameColumn != null || lastNameColumn != null)
            {
                if (firstNameColumn != null) selectColumns.Add(firstNameColumn);
                if (lastNameColumn != null) selectColumns.Add(lastNameColumn);
            }
            else
            {
                // Fallback to first non-id column
                selectColumns.Add(columns.FirstOrDefault(c => c.ToLower() != "id") ?? columns[0]);
            }

            if (emailColumn != null) selectColumns.Add(emailColumn);
            if (phoneColumn != null) selectColumns.Add(phoneColumn);
            if (certColumn != null) selectColumns.Add(certColumn);
            if (langColumn != null) selectColumns.Add(langColumn);

            var query = $"SELECT {string.Join(", ", selectColumns)} FROM {tableName} LIMIT 1000;";

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            var nameIndex = nameColumn != null ? selectColumns.IndexOf(nameColumn) : -1;
            var firstNameIndex = firstNameColumn != null ? selectColumns.IndexOf(firstNameColumn) : -1;
            var lastNameIndex = lastNameColumn != null ? selectColumns.IndexOf(lastNameColumn) : -1;
            var emailIndex = emailColumn != null ? selectColumns.IndexOf(emailColumn) : -1;
            var phoneIndex = phoneColumn != null ? selectColumns.IndexOf(phoneColumn) : -1;
            var certIndex = certColumn != null ? selectColumns.IndexOf(certColumn) : -1;
            var langIndex = langColumn != null ? selectColumns.IndexOf(langColumn) : -1;

            while (await reader.ReadAsync())
            {
                try
                {
                    string name = "";
                    if (nameIndex >= 0 && !reader.IsDBNull(nameIndex))
                    {
                        name = reader.GetString(nameIndex)?.Trim() ?? "";
                    }
                    else if (firstNameIndex >= 0 || lastNameIndex >= 0)
                    {
                        var firstName = firstNameIndex >= 0 && !reader.IsDBNull(firstNameIndex) ? reader.GetString(firstNameIndex)?.Trim() : "";
                        var lastName = lastNameIndex >= 0 && !reader.IsDBNull(lastNameIndex) ? reader.GetString(lastNameIndex)?.Trim() : "";
                        name = $"{firstName} {lastName}".Trim();
                    }

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var interpreter = new Interpreter
                    {
                        Name = name,
                        Email = emailIndex >= 0 && !reader.IsDBNull(emailIndex) ? reader.GetString(emailIndex)?.Trim() : null,
                        Phone = phoneIndex >= 0 && !reader.IsDBNull(phoneIndex) ? reader.GetString(phoneIndex)?.Trim() : null,
                        Certification = certIndex >= 0 && !reader.IsDBNull(certIndex) ? reader.GetString(certIndex)?.Trim() : "RID Member",
                        Languages = langIndex >= 0 && !reader.IsDBNull(langIndex) ? reader.GetString(langIndex)?.Trim() : "ASL",
                        Availability = "Contact for availability",
                        Notes = "Imported from RID database",
                        IsRegisteredWithAgency = false // RID directory interpreters are not yet registered
                    };

                    interpreters.Add(interpreter);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading row: {ex.Message}");
                }
            }

            Console.WriteLine($"Parsed {interpreters.Count} interpreters from RID database.");

            // Batch insert
            const int batchSize = 100;
            for (int i = 0; i < interpreters.Count; i += batchSize)
            {
                var batch = interpreters.Skip(i).Take(batchSize);
                await db.Interpreters.AddRangeAsync(batch);
                await db.SaveChangesAsync();
                Console.WriteLine($"Imported {Math.Min(i + batchSize, interpreters.Count)} of {interpreters.Count} interpreters...");
            }

            Console.WriteLine($"Successfully imported {interpreters.Count} interpreters from RID database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing interpreters: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
