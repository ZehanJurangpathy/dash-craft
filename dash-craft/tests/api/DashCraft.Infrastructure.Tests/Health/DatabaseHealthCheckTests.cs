using System.Diagnostics;
using DashCraft.Infrastructure.Health;
using DashCraft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace DashCraft.Infrastructure.Tests.Health;

public sealed class DatabaseHealthCheckTests
{
    [Fact]
    public async Task Reports_unhealthy_without_touching_the_database_while_not_ready()
    {
        var readiness = new DatabaseReadiness(Options.Create(new DatabaseOptions { ApplyMigrationsOnStartup = true }));
        await using var db = new DashCraftDbContext(
            new DbContextOptionsBuilder<DashCraftDbContext>().UseSqlServer("Server=unused;Database=DashCraft").Options);
        var check = new DatabaseHealthCheck(db, readiness);

        var stopwatch = Stopwatch.StartNew();
        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.ShouldBe(HealthStatus.Unhealthy);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }
}
