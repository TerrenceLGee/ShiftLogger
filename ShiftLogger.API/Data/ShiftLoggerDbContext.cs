using Microsoft.EntityFrameworkCore;
using ShiftLogger.API.Models;

namespace ShiftLogger.API.Data;

public class ShiftLoggerDbContext : DbContext
{
    public DbSet<Shift> Shifts { get; set; }
    public DbSet<Worker> Workers { get; set; }

    public ShiftLoggerDbContext(DbContextOptions<ShiftLoggerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Worker)
            .WithMany()
            .HasForeignKey(s => s.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
