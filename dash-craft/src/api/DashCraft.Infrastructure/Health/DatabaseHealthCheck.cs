using DashCraft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DashCraft.Infrastructure.Health;

/// <summary>
/// The "database" health check: Unhealthy until startup migrations have finished, then whether the
/// application database accepts connections. Never exposes exception details.
/// </summary>
internal sealed class DatabaseHealthCheck(DashCraftDbContext db, DatabaseReadiness readiness) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!readiness.IsReady)
        {
            return HealthCheckResult.Unhealthy();
        }

        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy();
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy();
        }
    }
}
