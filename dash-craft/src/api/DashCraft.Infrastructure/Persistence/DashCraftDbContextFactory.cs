using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DashCraft.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c>. Generating migrations never opens a connection,
/// so a non-secret placeholder connection string is enough.
/// </summary>
internal sealed class DashCraftDbContextFactory : IDesignTimeDbContextFactory<DashCraftDbContext>
{
    public DashCraftDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DashCraftDbContext>()
            .UseSqlServer("Server=localhost;Database=DashCraft;Trusted_Connection=False;TrustServerCertificate=True")
            .Options;

        return new DashCraftDbContext(options);
    }
}
