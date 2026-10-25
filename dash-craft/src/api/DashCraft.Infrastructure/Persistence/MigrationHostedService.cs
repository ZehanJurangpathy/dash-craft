using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DashCraft.Infrastructure.Persistence;

/// <summary>
/// Applies EF Core migrations in the background so the API starts listening immediately and
/// /health reports 503 until the database is ready.
/// </summary>
internal sealed partial class MigrationHostedService(
    IServiceScopeFactory scopeFactory,
    DatabaseReadiness readiness,
    IOptions<DatabaseOptions> options,
    IHostApplicationLifetime lifetime,
    ILogger<MigrationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield first so host startup (and Kestrel) never waits on the database.
        await Task.Yield();

        var settings = options.Value;
        if (!settings.ApplyMigrationsOnStartup)
        {
            return;
        }

        var maxAttempts = Math.Max(1, settings.StartupMaxRetries);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<DashCraftDbContext>();
                await db.Database.MigrateAsync(stoppingToken);

                readiness.MarkReady();
                LogUpToDate(logger);
                return;
            }
            catch (Exception ex) when (ex is SqlException or InvalidOperationException && !stoppingToken.IsCancellationRequested)
            {
                if (attempt == maxAttempts)
                {
                    // Exception type only: messages can contain server names.
                    LogGaveUp(logger, maxAttempts, ex.GetType().Name);
                    lifetime.StopApplication();
                    return;
                }

                LogWaiting(logger, attempt, maxAttempts);
                await Task.Delay(settings.StartupRetryDelay, stoppingToken);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Application database is up to date")]
    private static partial void LogUpToDate(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Waiting for application database (attempt {Attempt}/{Max})")]
    private static partial void LogWaiting(ILogger logger, int attempt, int max);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Application database was not reachable after {Max} attempts ({ErrorType}); stopping")]
    private static partial void LogGaveUp(ILogger logger, int max, string errorType);
}
