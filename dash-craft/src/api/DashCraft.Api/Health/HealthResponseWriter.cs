using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DashCraft.Api.Health;

/// <summary>
/// Writes the /health response defined in specs/001-project-foundation/contracts/health.openapi.yaml.
/// Messages come from a fixed map, never from check descriptions or exceptions, so the anonymous
/// endpoint cannot leak host names or credentials (constitution Principle II).
/// </summary>
public static class HealthResponseWriter
{
    private const string GenericUnavailableMessage = "This dependency is not available.";

    private static readonly Dictionary<string, string> UnavailableMessages = new(StringComparer.Ordinal)
    {
        ["database"] = "The application database is not reachable.",
    };

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var response = new HealthResponse(
            report.Status,
            Round(report.TotalDuration),
            [.. report.Entries.Select(entry => new HealthCheckResponse(
                entry.Key,
                entry.Value.Status,
                entry.Value.Status == HealthStatus.Healthy
                    ? null
                    : UnavailableMessages.GetValueOrDefault(entry.Key, GenericUnavailableMessage),
                Round(entry.Value.Duration)))]);

        context.Response.ContentType = "application/json; charset=utf-8";
        return JsonSerializer.SerializeAsync(context.Response.Body, response, SerializerOptions, context.RequestAborted);
    }

    private static double Round(TimeSpan duration) => Math.Round(duration.TotalMilliseconds, 1);

    private sealed record HealthResponse(HealthStatus Status, double TotalDurationMs, IReadOnlyList<HealthCheckResponse> Checks);

    private sealed record HealthCheckResponse(string Name, HealthStatus Status, string? Description, double DurationMs);
}
