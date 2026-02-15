using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public static class InterpreterMigrator
{
    public static async Task MigrateInterpretersTableAsync(AgencyDbContext db)
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
                    WHERE type='table' AND name='Interpreters';
                ";
                var tableExists = await checkCommand.ExecuteScalarAsync() != null;

                if (!tableExists)
                {
                    return;
                }

                // Get existing columns
                var existingColumns = new List<string>();
                using var pragmaCommand = connection.CreateCommand();
                pragmaCommand.CommandText = "PRAGMA table_info(Interpreters);";
                using var reader = await pragmaCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    existingColumns.Add(reader.GetString(1));
                }

                // Add IsRegisteredWithAgency column if it doesn't exist
                if (!existingColumns.Contains("IsRegisteredWithAgency"))
                {
                    try
                    {
                        using var alterCommand = connection.CreateCommand();
                        alterCommand.CommandText = @"
                            ALTER TABLE ""Interpreters"" 
                            ADD COLUMN ""IsRegisteredWithAgency"" INTEGER NOT NULL DEFAULT 0;
                        ";
                        await alterCommand.ExecuteNonQueryAsync();
                        Console.WriteLine("Added column IsRegisteredWithAgency to Interpreters table.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not add column IsRegisteredWithAgency: {ex.Message}");
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
            Console.WriteLine($"Warning: Could not migrate Interpreters table: {ex.Message}");
        }
    }
}
