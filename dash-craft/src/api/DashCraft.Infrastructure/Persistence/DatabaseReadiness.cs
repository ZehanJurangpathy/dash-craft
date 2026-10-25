using Microsoft.Extensions.Options;

namespace DashCraft.Infrastructure.Persistence;

/// <summary>
/// Tracks whether startup migrations have finished, so the health check can report Unhealthy
/// while the API is still waiting for the database.
/// </summary>
public sealed class DatabaseReadiness
{
    private int _isReady;

    public DatabaseReadiness(IOptions<DatabaseOptions> options)
    {
        // Nothing to wait for when migrations are not applied by this process.
        if (!options.Value.ApplyMigrationsOnStartup)
        {
            MarkReady();
        }
    }

    public bool IsReady => Volatile.Read(ref _isReady) == 1;

    public void MarkReady() => Volatile.Write(ref _isReady, 1);
}
