using Microsoft.Data.Sqlite;

// Try multiple possible paths
var possiblePaths = new[]
{
    @"f:\projects\agencycursor\AgencyCursor.WebApp\rid_interpreters.db",
    @"f:\projects\agencycursor\rid_interpreters.db",
    @"f:\projects\agencycursor\AgencyCursor.WebApp\bin\Debug\net8.0\rid_interpreters.db",
};

string? dbPath = null;
foreach (var path in possiblePaths)
{
    if (File.Exists(path))
    {
        dbPath = path;
        Console.WriteLine($"Found database at: {dbPath}");
        break;
    }
}

if (dbPath == null)
{
    Console.WriteLine("Database not found in any of these locations:");
    foreach (var path in possiblePaths)
    {
        Console.WriteLine($"  - {path}");
    }
    return;
}

using var connection = new SqliteConnection($"Data Source={dbPath}");
await connection.OpenAsync();

// List all tables
using var tablesCmd = connection.CreateCommand();
tablesCmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
Console.WriteLine("\nAll tables in database:");
var tables = new List<string>();
using (var reader = await tablesCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        var tableName = reader.GetString(0);
        tables.Add(tableName);
        Console.WriteLine($"  - {tableName}");
    }
}

if (!tables.Any())
{
    Console.WriteLine("No tables found in database!");
    return;
}

// Find the phone table (case insensitive)
var phoneTableName = tables.FirstOrDefault(t => t.Contains("phone", StringComparison.OrdinalIgnoreCase));

if (phoneTableName == null)
{
    Console.WriteLine("\nNo table with 'phone' in the name found!");
    Console.WriteLine("Looking at the main interpreter table instead...");
    
    // Use the first table that looks like an interpreter table
    phoneTableName = tables.FirstOrDefault(t => 
        t.Contains("interpreter", StringComparison.OrdinalIgnoreCase) ||
        t.Contains("member", StringComparison.OrdinalIgnoreCase) ||
        t.Contains("rid", StringComparison.OrdinalIgnoreCase)) ?? tables.First();
}

Console.WriteLine($"\nUsing table: {phoneTableName}");

// Check table structure
using var structureCmd = connection.CreateCommand();
structureCmd.CommandText = $"PRAGMA table_info({phoneTableName});";
Console.WriteLine($"\nTable structure:");
var columns = new List<string>();
using (var reader = await structureCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        var colName = reader.GetString(1);
        var colType = reader.GetString(2);
        columns.Add(colName);
        Console.WriteLine($"  - {colName} ({colType})");
    }
}

// Check if phone_type column exists
if (!columns.Any(c => c.Equals("phone_type", StringComparison.OrdinalIgnoreCase)))
{
    Console.WriteLine($"\nNo 'phone_type' column found in {phoneTableName}!");
    return;
}

// Check current values
using var selectCmd = connection.CreateCommand();
selectCmd.CommandText = $"SELECT DISTINCT phone_type FROM {phoneTableName} WHERE phone_type IS NOT NULL;";
Console.WriteLine($"\nCurrent phone_type values:");
using (var reader = await selectCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {reader.GetString(0)}");
    }
}

// Update all to 'mobile'
using var updateCmd = connection.CreateCommand();
updateCmd.CommandText = $"UPDATE {phoneTableName} SET phone_type = 'mobile';";
var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
Console.WriteLine($"\nUpdated {rowsAffected} rows to phone_type = 'mobile'");

// Verify
using var verifyCmd = connection.CreateCommand();
verifyCmd.CommandText = $"SELECT DISTINCT phone_type FROM {phoneTableName};";
Console.WriteLine($"\nNew phone_type values:");
using (var reader = await verifyCmd.ExecuteReaderAsync())
{
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  - {(reader.IsDBNull(0) ? "NULL" : reader.GetString(0))}");
    }
}

Console.WriteLine("\nDone!");
