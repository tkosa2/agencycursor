using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace AgencyCursor.Data;

public static class RidInterpreterService
{
    public static async Task<List<Dictionary<string, object?>>> SearchInterpretersAsync(
        string ridDbPath,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? city = null,
        string? state = null,
        string? zipCode = null,
        string? category = null,
        string? freelanceStatus = null,
        string? gender = null,
        List<string>? certificates = null,
        List<string>? additionalSkills = null,
        List<string>? ethnicities = null)
    {
        var results = new List<Dictionary<string, object?>>();

        if (!File.Exists(ridDbPath))
        {
            return results;
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
                return results;
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

            // Build WHERE clause
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            var firstNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("firstname") || c.ToLower() == "first");
            var lastNameColumn = columns.FirstOrDefault(c => c.ToLower().Contains("lastname") || c.ToLower() == "last");
            var nameColumn = columns.FirstOrDefault(c => c.ToLower() == "name" || c.ToLower().Contains("fullname"));
            var emailColumn = columns.FirstOrDefault(c => c.ToLower().Contains("email"));
            var cityColumn = columns.FirstOrDefault(c => c.ToLower().Contains("city"));
            var stateColumn = columns.FirstOrDefault(c => c.ToLower() == "state" || c.ToLower().Contains("state"));
            var zipColumn = columns.FirstOrDefault(c => c.ToLower().Contains("zip") || c.ToLower().Contains("postal"));

            if (!string.IsNullOrWhiteSpace(firstName) && firstNameColumn != null)
            {
                whereConditions.Add($"{firstNameColumn} LIKE @firstName");
                parameters["@firstName"] = $"%{firstName}%";
            }
            else if (!string.IsNullOrWhiteSpace(firstName) && nameColumn != null)
            {
                whereConditions.Add($"{nameColumn} LIKE @firstName");
                parameters["@firstName"] = $"%{firstName}%";
            }

            if (!string.IsNullOrWhiteSpace(lastName) && lastNameColumn != null)
            {
                whereConditions.Add($"{lastNameColumn} LIKE @lastName");
                parameters["@lastName"] = $"%{lastName}%";
            }
            else if (!string.IsNullOrWhiteSpace(lastName) && nameColumn != null)
            {
                whereConditions.Add($"{nameColumn} LIKE @lastName");
                parameters["@lastName"] = $"%{lastName}%";
            }

            if (!string.IsNullOrWhiteSpace(email) && emailColumn != null)
            {
                whereConditions.Add($"{emailColumn} LIKE @email");
                parameters["@email"] = $"%{email}%";
            }

            if (!string.IsNullOrWhiteSpace(city) && cityColumn != null)
            {
                whereConditions.Add($"{cityColumn} LIKE @city");
                parameters["@city"] = $"%{city}%";
            }

            if (!string.IsNullOrWhiteSpace(state) && stateColumn != null)
            {
                whereConditions.Add($"({stateColumn} LIKE @state OR {stateColumn} = @stateExact)");
                parameters["@state"] = $"%{state}%";
                parameters["@stateExact"] = state;
            }

            if (!string.IsNullOrWhiteSpace(zipCode) && zipColumn != null)
            {
                whereConditions.Add($"{zipColumn} LIKE @zipCode");
                parameters["@zipCode"] = $"{zipCode}%";
            }

            // Build SELECT with all available columns
            var selectColumns = columns.Where(c => !string.IsNullOrEmpty(c)).ToList();

            var query = $"SELECT {string.Join(", ", selectColumns)} FROM {tableName}";
            if (whereConditions.Any())
            {
                query += " WHERE " + string.Join(" AND ", whereConditions);
            }
            query += " LIMIT 1000;";

            using var command = connection.CreateCommand();
            command.CommandText = query;
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    row[columnName] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching RID interpreters: {ex.Message}");
        }

        return results;
    }
}
