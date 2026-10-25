namespace DashCraft.Infrastructure.Persistence;

/// <summary>Settings for the DashCraft application database (configuration section "Database").</summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Name of the connection string in the "ConnectionStrings" section.</summary>
    public const string ConnectionStringName = "DashCraft";

    /// <summary>Apply EF Core migrations in the background when the API starts.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }

    /// <summary>How many times to try migrating before giving up and stopping the host.</summary>
    public int StartupMaxRetries { get; set; } = 12;

    /// <summary>Delay between migration attempts while the database is not reachable yet.</summary>
    public TimeSpan StartupRetryDelay { get; set; } = TimeSpan.FromSeconds(5);
}
