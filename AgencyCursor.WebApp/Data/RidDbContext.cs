using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public class RidDbContext : DbContext
{
    private readonly string _connectionString;

    public RidDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }
}
