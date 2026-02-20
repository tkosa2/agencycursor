using Microsoft.Data.Sqlite;

var dbPath = @"f:\projects\agencycursor\rid_interpreters.db";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"Database not found: {dbPath}");
    return;
}

using var connection = new SqliteConnection($"Data Source={dbPath}");
await connection.OpenAsync();

// Check current values
using var selectCmd = connection.CreateCommand();
selectCmd.CommandText = "SELECT DISTINCT phone_type FROM interpreter_phones;";
Console.WriteLine("Current phone_type values:");
using (var reader = await selectCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {(reader.IsDBNull(0) ? "NULL" : reader.GetString(0))}");
    }
}

// Update all to 'mobile'
using var updateCmd = connection.CreateCommand();
updateCmd.CommandText = "UPDATE interpreter_phones SET phone_type = 'mobile';";
var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
Console.WriteLine($"\nUpdated {rowsAffected} rows to phone_type = 'mobile'");

// Verify
using var verifyCmd = connection.CreateCommand();
verifyCmd.CommandText = "SELECT DISTINCT phone_type FROM interpreter_phones;";
Console.WriteLine("\nNew phone_type values:");
using (var reader = await verifyCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {(reader.IsDBNull(0) ? "NULL" : reader.GetString(0))}");
    }
}

Console.WriteLine("\nDone!");
