using DashCraft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DashCraft.Infrastructure.Tests.Persistence;

public sealed class DashCraftDbContextTests
{
    private static DashCraftDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<DashCraftDbContext>().UseSqlServer("Server=unused;Database=DashCraft").Options);

    [Fact]
    public void Model_uses_the_dbo_schema()
    {
        using var db = CreateContext();

        db.Model.GetDefaultSchema().ShouldBe("dbo");
    }

    [Fact]
    public void Has_exactly_the_initial_baseline_migration()
    {
        using var db = CreateContext();

        db.Database.GetMigrations().ShouldHaveSingleItem().ShouldEndWith("_InitialCreate");
    }
}
