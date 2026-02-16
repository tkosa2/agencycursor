using AgencyCursor.Models;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public static class SeedData
{
    public static async Task EnsureSeedDataAsync(AgencyDbContext db, string? contentRootPath = null)
    {
        // Always ensure admin requestor exists
        await EnsureAdminRequestorAsync(db);
        
        var hasRequestors = await db.Requestors.AnyAsync();
        var hasRegisteredInterpreters = await db.Interpreters.AnyAsync(i => i.IsRegisteredWithAgency);

        // Only seed requestors if database is empty
        if (!hasRequestors)
        {
            Console.WriteLine("Generating mock data...");

            // Generate requestors
            var requestors = MockDataGenerator.GenerateRequestors();
            db.Requestors.AddRange(requestors);
            await db.SaveChangesAsync();
            Console.WriteLine($"Created {requestors.Count} requestors.");

            // Import interpreters from RID database
            // Use ContentRootPath if provided, otherwise fall back to current directory
            var basePath = contentRootPath ?? Directory.GetCurrentDirectory();
            var ridDbPath = RidDbHelper.GetRidDbPath(basePath);
            if (File.Exists(ridDbPath))
            {
                await RidInterpreterImporter.ImportInterpretersAsync(db, ridDbPath);
                
                // Register first 10 Washington state interpreters
                await InterpreterRegistration.RegisterWashingtonInterpretersAsync(db, ridDbPath);
            }
            else
            {
                Console.WriteLine($"RID interpreters database not found at {ridDbPath}");
            }

            // Get all interpreters for generating requests
            var interpreters = await db.Interpreters.ToListAsync();
            Console.WriteLine($"Using {interpreters.Count} interpreters for request generation.");

            // Generate requests
            var requests = MockDataGenerator.GenerateRequests(requestors, interpreters);
            db.Requests.AddRange(requests);
            await db.SaveChangesAsync();
            Console.WriteLine($"Created {requests.Count} requests.");

            // Generate appointments
            var appointments = MockDataGenerator.GenerateAppointments(requests, interpreters);
            db.Appointments.AddRange(appointments);
            await db.SaveChangesAsync();
            Console.WriteLine($"Created {appointments.Count} appointments.");

            // Generate invoices
            var invoices = MockDataGenerator.GenerateInvoices(appointments, requestors, interpreters, requests);
            db.Invoices.AddRange(invoices);
            await db.SaveChangesAsync();
            Console.WriteLine($"Created {invoices.Count} invoices.");

            Console.WriteLine("Mock data generation complete!");
        }

        // Always ensure there are registered interpreters (even if database already has data)
        if (!hasRegisteredInterpreters)
        {
            Console.WriteLine("No registered interpreters found. Creating mock RID-imported interpreters...");
            var mockRidInterpreters = MockDataGenerator.GenerateRidImportedInterpreters();
            db.Interpreters.AddRange(mockRidInterpreters);
            await db.SaveChangesAsync();
            Console.WriteLine($"Created {mockRidInterpreters.Count} mock RID-imported interpreters.");
        }
    }

    private static async Task EnsureAdminRequestorAsync(AgencyDbContext db)
    {
        const string adminRequestorName = "Admin";
        var adminRequestor = await db.Requestors
            .FirstOrDefaultAsync(r => r.Name == adminRequestorName);
        
        if (adminRequestor == null)
        {
            adminRequestor = new Requestor
            {
                Name = adminRequestorName,
                Email = "admin@agency.example.com",
                Phone = "+1 (555) 000-0000",
                Address = "Administrative Office",
                Notes = "System requestor for admin-created appointments"
            };
            db.Requestors.Add(adminRequestor);
            await db.SaveChangesAsync();
            Console.WriteLine("Created admin requestor.");
        }
    }
}
