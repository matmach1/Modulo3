using Microsoft.EntityFrameworkCore;
using Riesgos.Api.Domain;

namespace Riesgos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Email).IsUnique();
            user.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Report>(report =>
        {
            report.Property(r => r.SuggestedCategory).HasConversion<string>();
            report.Property(r => r.Type).HasConversion<string>();
            report.Property(r => r.Category).HasConversion<string>();
            report.Property(r => r.Criticality).HasConversion<string>();
            report.Property(r => r.Status).HasConversion<string>();
            report.HasIndex(r => r.CreatedAt);
        });
    }
}
