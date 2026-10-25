using System.Net;
using System.Text.Json;
using DashCraft.IntegrationTests.Infrastructure;

namespace DashCraft.IntegrationTests.Health;

/// <summary>FR-005: /health reports 503 when the database is unreachable, without leaking details.</summary>
public sealed class DatabaseUnavailableTests
{
    internal const string UnreachableConnectionString =
        "Server=127.0.0.1,1;Database=DashCraft;User Id=sa;Password=x;Connect Timeout=2;TrustServerCertificate=True";

    [Fact]
    public async Task Reports_503_with_an_unhealthy_database_and_no_leaked_details()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new DashCraftApiFactory(UnreachableConnectionString, applyMigrations: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var raw = await response.Content.ReadAsStringAsync(ct);
        using var json = JsonDocument.Parse(raw);
        json.RootElement.GetProperty("status").GetString().ShouldBe("Unhealthy");
        var check = json.RootElement.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Unhealthy");
        check.GetProperty("description").GetString().ShouldBe("The application database is not reachable.");
        raw.ShouldNotContain("127.0.0.1");
        raw.ShouldNotContain("Password");
        raw.ShouldNotContain("Exception");
    }
}
