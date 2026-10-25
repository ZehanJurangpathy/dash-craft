using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace DashCraft.IntegrationTests.Infrastructure;

/// <summary>
/// One real SQL Server container shared by every test class in <see cref="SqlServerCollection"/>.
/// Each test gets its own database on it (see <see cref="UniqueDatabaseName"/>) so tests never
/// interfere. Tests that don't need SQL Server stay outside the collection and run without Docker.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string Image = "mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-24.04";

    private MsSqlContainer? _container;

    public async ValueTask InitializeAsync()
    {
        try
        {
            _container = new MsSqlBuilder(Image).Build();
            await _container.StartAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Integration tests need a running Docker engine. Start Docker Desktop and try again.", ex);
        }
    }

    /// <summary>Connection string for a database on the shared server (created by migrations if missing).</summary>
    public string CreateConnectionString(string database)
    {
        if (_container is null)
        {
            throw new InvalidOperationException("SQL Server container has not been started.");
        }

        return new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = database }.ConnectionString;
    }

    /// <summary>A unique database name, so parallel tests never migrate the same database.</summary>
    public static string UniqueDatabaseName() => $"DashCraft_{Guid.NewGuid():N}";

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}
