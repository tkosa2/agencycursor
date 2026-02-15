using AgencyCursor.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public static class InterpreterRegistration
{
    public static async Task RegisterWashingtonInterpretersAsync(AgencyDbContext db, string ridDbPath)
    {
        if (!File.Exists(ridDbPath))
        {
            Console.WriteLine("RID database not found. Cannot register Washington interpreters.");
            return;
        }

        try
        {
            // First, try to find Washington interpreters from the RID database
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
                Console.WriteLine("No suitable table found in RID database.");
                return;
            }

            // Get column names
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

            // Find state column - try various common names
            var stateColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "state" ||
                c.ToLower().Contains("state") || 
                c.ToLower().Contains("province") ||
                c.ToLower() == "st" ||
                c.ToLower() == "region");

            if (stateColumn == null)
            {
                Console.WriteLine("No state column found in RID database. Registering first 10 interpreters...");
                // Fallback: register first 10 interpreters
                await RegisterFirst10InterpretersAsync(db);
                return;
            }

            // Find name column
            var nameColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "name" || 
                c.ToLower().Contains("fullname") ||
                (c.ToLower().Contains("name") && !c.ToLower().Contains("first") && !c.ToLower().Contains("last")));

            var firstNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("firstname") || c.ToLower() == "first");
            var lastNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lastname") || c.ToLower() == "last");

            if (nameColumn == null && firstNameColumn == null)
            {
                Console.WriteLine("No name column found in RID database.");
                return;
            }

            // Query for Washington state interpreters
            var selectColumns = new List<string>();
            if (nameColumn != null) selectColumns.Add(nameColumn);
            if (firstNameColumn != null) selectColumns.Add(firstNameColumn);
            if (lastNameColumn != null) selectColumns.Add(lastNameColumn);
            selectColumns.Add(stateColumn);

            // Query for Washington state interpreters - try various formats
            var query = $@"SELECT {string.Join(", ", selectColumns)} FROM {tableName} 
                          WHERE UPPER({stateColumn}) LIKE '%WA%' 
                             OR UPPER({stateColumn}) LIKE '%WASHINGTON%'
                             OR {stateColumn} = 'WA'
                             OR {stateColumn} = 'Washington'
                          LIMIT 10;";

            using var command = connection.CreateCommand();
            command.CommandText = query;

            var washingtonNames = new List<string>();
            using var reader = await command.ExecuteReaderAsync();
            var nameIndex = nameColumn != null ? selectColumns.IndexOf(nameColumn) : -1;
            var firstNameIndex = firstNameColumn != null ? selectColumns.IndexOf(firstNameColumn) : -1;
            var lastNameIndex = lastNameColumn != null ? selectColumns.IndexOf(lastNameColumn) : -1;

            while (await reader.ReadAsync())
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

                if (!string.IsNullOrWhiteSpace(name))
                {
                    washingtonNames.Add(name);
                }
            }

            Console.WriteLine($"Found {washingtonNames.Count} Washington state interpreters in RID database.");

            // Update interpreters in agency database
            var updated = 0;
            foreach (var name in washingtonNames.Take(10))
            {
                var interpreter = await db.Interpreters
                    .FirstOrDefaultAsync(i => i.Name == name);

                if (interpreter != null && !interpreter.IsRegisteredWithAgency)
                {
                    interpreter.IsRegisteredWithAgency = true;
                    updated++;
                }
            }

            if (updated > 0)
            {
                await db.SaveChangesAsync();
                Console.WriteLine($"Registered {updated} Washington state interpreters with the agency.");
            }
            else
            {
                Console.WriteLine("No matching interpreters found in agency database to register.");
                // Fallback: just register first 10
                await RegisterFirst10InterpretersAsync(db);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering Washington interpreters: {ex.Message}");
            // Fallback: just register first 10
            await RegisterFirst10InterpretersAsync(db);
        }
    }

    private static async Task RegisterFirst10InterpretersAsync(AgencyDbContext db)
    {
        var interpreters = await db.Interpreters
            .Where(i => !i.IsRegisteredWithAgency)
            .Take(10)
            .ToListAsync();

        foreach (var interpreter in interpreters)
        {
            interpreter.IsRegisteredWithAgency = true;
        }

        if (interpreters.Count > 0)
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"Registered first {interpreters.Count} interpreters with the agency.");
        }
    }
}
