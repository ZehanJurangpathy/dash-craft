using Microsoft.EntityFrameworkCore;

namespace DashCraft.Infrastructure.Persistence;

/// <summary>
/// EF Core context for DashCraft's own application database. It never targets user-connected
/// databases (constitution, Technology Standards).
/// </summary>
public sealed class DashCraftDbContext(DbContextOptions<DashCraftDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureAssembly).Assembly);
    }
}
