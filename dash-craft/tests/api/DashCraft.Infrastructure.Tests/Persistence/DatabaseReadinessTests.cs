using DashCraft.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace DashCraft.Infrastructure.Tests.Persistence;

public sealed class DatabaseReadinessTests
{
    [Fact]
    public void Is_ready_immediately_when_migrations_are_not_applied_on_startup()
    {
        var readiness = new DatabaseReadiness(Options.Create(new DatabaseOptions { ApplyMigrationsOnStartup = false }));

        readiness.IsReady.ShouldBeTrue();
    }

    [Fact]
    public void Is_not_ready_until_marked_when_migrations_run_on_startup()
    {
        var readiness = new DatabaseReadiness(Options.Create(new DatabaseOptions { ApplyMigrationsOnStartup = true }));

        readiness.IsReady.ShouldBeFalse();
        readiness.MarkReady();
        readiness.IsReady.ShouldBeTrue();
    }
}
