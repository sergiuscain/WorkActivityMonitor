using Microsoft.EntityFrameworkCore;
using WorkActivityMonitor.Models;

namespace WorkActivityMonitor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Screenshot> Screenshots => Set<Screenshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => new { c.MachineName, c.UserName, c.Domain }).IsUnique();
            e.Property(c => c.MachineName).HasMaxLength(128).IsRequired();
            e.Property(c => c.UserName).HasMaxLength(128).IsRequired();
            e.Property(c => c.Domain).HasMaxLength(128);
            e.Property(c => c.IpAddress).HasMaxLength(64);
        });

        modelBuilder.Entity<Screenshot>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Client)
             .WithMany(c => c.Screenshots)
             .HasForeignKey(s => s.ClientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(s => s.FilePath).HasMaxLength(512).IsRequired();
        });
    }
}
