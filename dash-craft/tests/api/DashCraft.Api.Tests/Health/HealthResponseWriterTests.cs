using System.Text.Json;
using DashCraft.Api.Health;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DashCraft.Api.Tests.Health;

/// <summary>
/// Contract tests for GET /health (specs/001-project-foundation/contracts/health.openapi.yaml).
/// This is also the no-leak test required by constitution Principle II, Health endpoint exception.
/// </summary>
public sealed class HealthResponseWriterTests
{
    [Fact]
    public async Task Writes_the_contract_shape_as_json()
    {
        var report = Report(HealthStatus.Healthy,
            ("database", new HealthReportEntry(HealthStatus.Healthy, null, TimeSpan.FromMilliseconds(11.94), null, null)));

        var (context, body) = await WriteAsync(report);

        context.Response.ContentType.ShouldBe("application/json; charset=utf-8");
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["status", "totalDurationMs", "checks"]);
        body.GetProperty("status").GetString().ShouldBe("Healthy");
        body.GetProperty("totalDurationMs").GetDouble().ShouldBe(12.4);

        var check = body.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        check.EnumerateObject().Select(p => p.Name).ShouldBe(["name", "status", "durationMs"]);
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Healthy");
        check.GetProperty("durationMs").GetDouble().ShouldBe(11.9);
    }

    [Theory]
    [InlineData(HealthStatus.Degraded, "Degraded")]
    [InlineData(HealthStatus.Unhealthy, "Unhealthy")]
    public async Task Writes_statuses_as_strings(HealthStatus status, string expected)
    {
        var report = Report(status, ("database", new HealthReportEntry(status, null, TimeSpan.Zero, null, null)));

        var (_, body) = await WriteAsync(report);

        body.GetProperty("status").GetString().ShouldBe(expected);
    }

    [Fact]
    public async Task Unhealthy_database_uses_a_fixed_message_and_leaks_no_details()
    {
        var exception = new InvalidOperationException("Login failed for user 'sa'. Server=secret-host");
        var report = Report(HealthStatus.Unhealthy, ("database", new HealthReportEntry(
            HealthStatus.Unhealthy, "Server=secret-host refused", TimeSpan.FromSeconds(5), exception, null)));

        var (_, body) = await WriteAsync(report);

        var check = body.GetProperty("checks")[0];
        check.GetProperty("description").GetString().ShouldBe("The application database is not reachable.");
        var raw = body.GetRawText();
        raw.ShouldNotContain("Login failed");
        raw.ShouldNotContain("secret-host");
        raw.ShouldNotContain("Exception");
    }

    [Fact]
    public async Task Unknown_unhealthy_dependency_gets_a_generic_message()
    {
        var report = Report(HealthStatus.Unhealthy,
            ("cache", new HealthReportEntry(HealthStatus.Unhealthy, "details", TimeSpan.Zero, null, null)));

        var (_, body) = await WriteAsync(report);

        body.GetProperty("checks")[0].GetProperty("description").GetString().ShouldBe("This dependency is not available.");
    }

    private static HealthReport Report(HealthStatus status, params (string Name, HealthReportEntry Entry)[] entries) =>
        new(entries.ToDictionary(e => e.Name, e => e.Entry), status, TimeSpan.FromMilliseconds(12.36));

    private static async Task<(HttpContext Context, JsonElement Body)> WriteAsync(HealthReport report)
    {
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        await HealthResponseWriter.WriteAsync(context, report);
        context.Response.Body.Position = 0;
        return (context, JsonDocument.Parse(context.Response.Body).RootElement.Clone());
    }
}
