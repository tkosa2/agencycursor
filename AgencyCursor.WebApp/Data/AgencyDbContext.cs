using AgencyCursor.Models;
using Microsoft.EntityFrameworkCore;

namespace AgencyCursor.Data;

public class AgencyDbContext : DbContext
{
    public AgencyDbContext(DbContextOptions<AgencyDbContext> options)
        : base(options) { }

    public DbSet<Requestor> Requestors => Set<Requestor>();
    public DbSet<Interpreter> Interpreters => Set<Interpreter>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ZipCode> ZipCodes => Set<ZipCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Request>()
            .HasOne(r => r.Requestor)
            .WithMany(req => req.Requests)
            .HasForeignKey(r => r.RequestorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.PreferredInterpreter)
            .WithMany(i => i.PreferredForRequests)
            .HasForeignKey(r => r.PreferredInterpreterId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Request)
            .WithMany(r => r.Appointments)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Interpreter)
            .WithMany(i => i.Appointments)
            .HasForeignKey(a => a.InterpreterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Requestor)
            .WithMany(r => r.Invoices)
            .HasForeignKey(i => i.RequestorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Appointment)
            .WithMany(a => a.Invoices)
            .HasForeignKey(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Interpreter)
            .WithMany(interp => interp.Invoices)
            .HasForeignKey(i => i.InterpreterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
