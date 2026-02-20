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
    public DbSet<AppointmentInterpreter> AppointmentInterpreters => Set<AppointmentInterpreter>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
    public DbSet<InterpreterResponse> InterpreterResponses => Set<InterpreterResponse>();
    public DbSet<InterpreterEmailLog> InterpreterEmailLogs => Set<InterpreterEmailLog>();

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

        // Old relationship - kept for backwards compatibility during migration
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Interpreter)
            .WithMany()
            .HasForeignKey(a => a.InterpreterId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // New many-to-many relationship through AppointmentInterpreter
        modelBuilder.Entity<AppointmentInterpreter>()
            .HasOne(ai => ai.Appointment)
            .WithMany(a => a.AppointmentInterpreters)
            .HasForeignKey(ai => ai.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppointmentInterpreter>()
            .HasOne(ai => ai.Interpreter)
            .WithMany(i => i.AppointmentInterpreters)
            .HasForeignKey(ai => ai.InterpreterId)
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
