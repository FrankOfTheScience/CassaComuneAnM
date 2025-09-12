using CassaComuneAnM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CassaComuneAnM.Infrastructure.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options) { }

    public DbSet<Trip> Trips { get; set; }
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Deposit> Deposits { get; set; }
    public DbSet<ExpenseParticipant> ExpenseParticipants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trip>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Participant>()
           .Property(p => p.Id)
           .ValueGeneratedOnAdd();

        modelBuilder.Entity<Expense>()
              .Property(e => e.Id)
              .ValueGeneratedOnAdd();

        modelBuilder.Entity<Deposit>()
                .Property(d => d.Id)
                .ValueGeneratedOnAdd();

        // Configurazione della relazione many-to-many tra Expense e Participant tramite ExpenseParticipant
        modelBuilder.Entity<ExpenseParticipant>()
            .HasKey(ep => new { ep.ExpenseId, ep.ParticipantId });

        modelBuilder.Entity<ExpenseParticipant>()
            .HasOne(ep => ep.Expense)
            .WithMany(e => e.ExpenseParticipants)
            .HasForeignKey(ep => ep.ExpenseId);

        modelBuilder.Entity<ExpenseParticipant>()
            .HasOne(ep => ep.Participant)
            .WithMany(p => p.ExpenseParticipants)
            .HasForeignKey(ep => ep.ParticipantId);

    }
}
