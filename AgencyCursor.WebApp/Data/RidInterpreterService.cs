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

            // Debug: Log available columns (only in development)
            if (columns.Any())
            {
                Console.WriteLine($"Found {columns.Count} columns in {tableName}: {string.Join(", ", columns.Take(20))}");
            }

            // Build WHERE clause
            var whereConditions = new List<string>();
            var parameters = new Dictionary<string, object?>();

            // Try multiple patterns for each column to handle various naming conventions
            var firstNameColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "firstname" || 
                c.ToLower() == "first_name" ||
                c.ToLower() == "first" ||
                c.ToLower().Contains("firstname") ||
                c.ToLower().Contains("first_name"));
            var lastNameColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "lastname" || 
                c.ToLower() == "last_name" ||
                c.ToLower() == "last" ||
                c.ToLower().Contains("lastname") ||
                c.ToLower().Contains("last_name"));
            var nameColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "name" || 
                c.ToLower() == "fullname" ||
                c.ToLower() == "full_name" ||
                c.ToLower().Contains("fullname") ||
                (c.ToLower().Contains("name") && !c.ToLower().Contains("first") && !c.ToLower().Contains("last")));
            var emailColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "email" ||
                c.ToLower() == "emailaddress" ||
                c.ToLower() == "email_address" ||
                c.ToLower().Contains("email"));
            var cityColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "city" ||
                c.ToLower().Contains("city"));
            // Find state column - try various common names
            var stateColumn = columns.FirstOrDefault(c => 
                c.ToLower() == "state" ||
                c.ToLower().Contains("state") || 
                c.ToLower().Contains("province") ||
                c.ToLower() == "st" ||
                c.ToLower() == "region");
            var zipColumn = columns.FirstOrDefault(c => c.ToLower().Contains("zip") || c.ToLower().Contains("postal"));
            var genderColumn = columns.FirstOrDefault(c => 
                c.ToLower().Contains("gender") || 
                c.ToLower().Contains("sex"));
            var categoryColumn = columns.FirstOrDefault(c => 
                c.ToLower().Contains("category") || 
                c.ToLower().Contains("certification") ||
                c.ToLower().Contains("cert"));
            var freelanceColumn = columns.FirstOrDefault(c => 
                c.ToLower().Contains("freelance") || 
                c.ToLower().Contains("freelancer") ||
                c.ToLower().Contains("selfemployed"));

            if (!string.IsNullOrWhiteSpace(firstName) && firstNameColumn != null)
            {
                whereConditions.Add($"LOWER({firstNameColumn}) LIKE LOWER(@firstName)");
                parameters["@firstName"] = $"%{firstName}%";
            }
            else if (!string.IsNullOrWhiteSpace(firstName) && nameColumn != null)
            {
                whereConditions.Add($"LOWER({nameColumn}) LIKE LOWER(@firstName)");
                parameters["@firstName"] = $"%{firstName}%";
            }

            if (!string.IsNullOrWhiteSpace(lastName) && lastNameColumn != null)
            {
                whereConditions.Add($"LOWER({lastNameColumn}) LIKE LOWER(@lastName)");
                parameters["@lastName"] = $"%{lastName}%";
            }
            else if (!string.IsNullOrWhiteSpace(lastName) && nameColumn != null)
            {
                whereConditions.Add($"LOWER({nameColumn}) LIKE LOWER(@lastName)");
                parameters["@lastName"] = $"%{lastName}%";
            }

            if (!string.IsNullOrWhiteSpace(email) && emailColumn != null)
            {
                whereConditions.Add($"LOWER({emailColumn}) LIKE LOWER(@email)");
                parameters["@email"] = $"%{email}%";
            }

            if (!string.IsNullOrWhiteSpace(city) && cityColumn != null)
            {
                whereConditions.Add($"LOWER({cityColumn}) LIKE LOWER(@city)");
                parameters["@city"] = $"%{city}%";
            }

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

            if (!string.IsNullOrWhiteSpace(zipCode) && zipColumn != null)
            {
                whereConditions.Add($"{zipColumn} LIKE @zipCode");
                parameters["@zipCode"] = $"{zipCode}%";
            }

            if (!string.IsNullOrWhiteSpace(gender) && genderColumn != null)
            {
                // Gender can be stored as numeric codes or text values
                whereConditions.Add($"({genderColumn} = @gender OR LOWER({genderColumn}) LIKE LOWER(@genderLike))");
                parameters["@gender"] = gender;
                parameters["@genderLike"] = $"%{gender}%";
            }

            if (!string.IsNullOrWhiteSpace(category) && categoryColumn != null)
            {
                whereConditions.Add($"LOWER({categoryColumn}) LIKE LOWER(@category)");
                parameters["@category"] = $"%{category}%";
            }

            if (!string.IsNullOrWhiteSpace(freelanceStatus) && freelanceColumn != null)
            {
                // Freelance status might be stored as 1/0, Yes/No, true/false, etc.
                var freelanceValue = freelanceStatus.Trim().ToLower();
                if (freelanceValue == "1" || freelanceValue == "yes" || freelanceValue == "true")
                {
                    whereConditions.Add($"({freelanceColumn} = 1 OR LOWER({freelanceColumn}) LIKE LOWER(@freelanceYes) OR {freelanceColumn} = @freelanceYesExact)");
                    parameters["@freelanceYes"] = "%yes%";
                    parameters["@freelanceYesExact"] = "Yes";
                }
                else if (freelanceValue == "0" || freelanceValue == "no" || freelanceValue == "false")
                {
                    whereConditions.Add($"({freelanceColumn} = 0 OR LOWER({freelanceColumn}) LIKE LOWER(@freelanceNo) OR {freelanceColumn} = @freelanceNoExact)");
                    parameters["@freelanceNo"] = "%no%";
                    parameters["@freelanceNoExact"] = "No";
                }
                else
                {
                    whereConditions.Add($"LOWER({freelanceColumn}) LIKE LOWER(@freelance)");
                    parameters["@freelance"] = $"%{freelanceStatus}%";
                }
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
