using System.Net;
using System.Text.Json;
using DashCraft.IntegrationTests.Infrastructure;

namespace DashCraft.IntegrationTests.Health;

[Collection(SqlServerCollection.Name)]
public sealed class HealthEndpointTests(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task Reports_healthy_with_the_database_check_once_migrations_have_run()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new DashCraftApiFactory(sqlServer.CreateConnectionString(SqlServerFixture.UniqueDatabaseName()));

        using var response = await factory.WaitUntilHealthyAsync(cancellationToken: ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var body = json.RootElement;
        body.GetProperty("status").GetString().ShouldBe("Healthy");
        body.GetProperty("totalDurationMs").GetDouble().ShouldBeGreaterThanOrEqualTo(0);
        var check = body.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Healthy");
    }
}
