using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace AgencyCursor.Data;

public static class DatabaseMigrator
{
    public static async Task MigrateRequestsTableAsync(AgencyDbContext db)
    {
        try
        {
            // Check if Requests table exists
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            try
            {
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = @"
                    SELECT name FROM sqlite_master 
                    WHERE type='table' AND name='Requests';
                ";
                var tableExists = await checkCommand.ExecuteScalarAsync() != null;

                if (!tableExists)
                {
                    // Table doesn't exist, EnsureCreated will handle it
                    return;
                }

                // Get existing columns
                var existingColumns = new List<string>();
                using var pragmaCommand = connection.CreateCommand();
                pragmaCommand.CommandText = "PRAGMA table_info(Requests);";
                using var reader = await pragmaCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1)); // Column name is at index 1
                }

                // Add missing columns
                var columnsToAdd = new Dictionary<string, string>
                {
                    { "RequestName", "TEXT" },
                    { "FirstName", "TEXT" },
                    { "LastName", "TEXT" },
                    { "NumberOfIndividuals", "INTEGER NOT NULL DEFAULT 1" },
                    { "IndividualType", "TEXT" },
                    { "TypeOfServiceOther", "TEXT" },
                    { "Mode", "TEXT" },
                    { "MeetingLink", "TEXT" },
                    { "Address", "TEXT" },
                    { "Address2", "TEXT" },
                    { "City", "TEXT" },
                    { "State", "TEXT" },
                    { "ZipCode", "TEXT" },
                    { "GenderPreference", "TEXT" },
                    { "PreferredInterpreterName", "TEXT" },
                    { "Specializations", "TEXT" },
                    { "EndDateTime", "TEXT" }
                };

                foreach (var column in columnsToAdd)
                {
                    if (!existingColumns.Contains(column.Key))
                    {
                        try
                        {
                            using var alterCommand = connection.CreateCommand();
                            alterCommand.CommandText = $@"
                                ALTER TABLE ""Requests"" 
                                ADD COLUMN ""{column.Key}"" {column.Value};
                            ";
                            await alterCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Added column {column.Key} to Requests table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not add column {column.Key}: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not migrate Requests table: {ex.Message}");
        }
    }

    public static async Task MigrateRequestorsTableAsync(AgencyDbContext db)
    {
        try
        {
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();
            try
            {
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = @"
                    SELECT name FROM sqlite_master 
                    WHERE type='table' AND name='Requestors';
                ";
                var tableExists = await checkCommand.ExecuteScalarAsync() != null;

                if (!tableExists)
                {
                    return;
                }

                // Get existing columns
                var existingColumns = new List<string>();
                using var pragmaCommand = connection.CreateCommand();
                pragmaCommand.CommandText = "PRAGMA table_info(Requestors);";
                using var reader = await pragmaCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1));
                }

                // Add missing columns
                var columnsToAdd = new Dictionary<string, string>
                {
                    { "FirstName", "TEXT" },
                    { "LastName", "TEXT" }
                };

                foreach (var column in columnsToAdd)
                {
                    if (!existingColumns.Contains(column.Key))
                    {
                        try
                        {
                            using var alterCommand = connection.CreateCommand();
                            alterCommand.CommandText = $@"
                                ALTER TABLE ""Requestors"" 
                                ADD COLUMN ""{column.Key}"" {column.Value};
                            ";
                            await alterCommand.ExecuteNonQueryAsync();
                            Console.WriteLine($"Added column {column.Key} to Requestors table.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Warning: Could not add column {column.Key}: {ex.Message}");
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not migrate Requestors table: {ex.Message}");
        }
    }
}
