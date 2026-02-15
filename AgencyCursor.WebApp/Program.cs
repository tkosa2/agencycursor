using AgencyCursor.Data;
using AgencyCursor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<AgencyDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<InvoicePdfService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgencyDbContext>();
    db.Database.EnsureCreated();
    
    // Migrate Requests table to add new columns
    DatabaseMigrator.MigrateRequestsTableAsync(db).GetAwaiter().GetResult();
    
    // Migrate Interpreters table to add IsRegisteredWithAgency column
    InterpreterMigrator.MigrateInterpretersTableAsync(db).GetAwaiter().GetResult();
    
    SeedData.EnsureSeedDataAsync(db).GetAwaiter().GetResult();
    
    // Import zip codes from dataset
    var datasetPath = Path.Combine(builder.Environment.ContentRootPath, "dataset", "ziplatlong2026.02.14.txt");
    if (File.Exists(datasetPath))
    {
        ZipCodeImporter.ImportZipCodesAsync(db, datasetPath).GetAwaiter().GetResult();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
