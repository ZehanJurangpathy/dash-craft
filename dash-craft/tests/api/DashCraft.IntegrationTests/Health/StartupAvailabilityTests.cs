using System.Net;
using System.Text.Json;
using DashCraft.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DashCraft.IntegrationTests.Health;

/// <summary>Spec edge case: while the database is not ready at startup, /health answers 503.</summary>
public sealed class StartupAvailabilityTests
{
    [Fact]
    public async Task Api_serves_503_while_startup_migrations_are_still_retrying()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new DashCraftApiFactory(
            DatabaseUnavailableTests.UnreachableConnectionString, applyMigrations: true, startupMaxRetries: 30);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var check = json.RootElement.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Unhealthy");

        var lifetime = factory.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.IsCancellationRequested.ShouldBeFalse();
    }
}
