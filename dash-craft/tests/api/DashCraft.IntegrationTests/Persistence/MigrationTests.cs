using DashCraft.IntegrationTests.Infrastructure;
using Microsoft.Data.SqlClient;

namespace DashCraft.IntegrationTests.Persistence;

[Collection(SqlServerCollection.Name)]
public sealed class MigrationTests(SqlServerFixture sqlServer)
{
    [Fact]
    public async Task Startup_applies_the_baseline_migration_once_and_is_repeatable()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionString = sqlServer.CreateConnectionString(SqlServerFixture.UniqueDatabaseName());

        await using (var first = new DashCraftApiFactory(connectionString))
        {
            (await first.WaitUntilHealthyAsync(cancellationToken: ct)).Dispose();
        }

        (await ReadMigrationIdsAsync(connectionString, ct)).ShouldHaveSingleItem().ShouldEndWith("_InitialCreate");

        // A second start against the same database must not re-apply or fail (FR-006).
        await using (var second = new DashCraftApiFactory(connectionString))
        {
            (await second.WaitUntilHealthyAsync(cancellationToken: ct)).Dispose();
        }

        (await ReadMigrationIdsAsync(connectionString, ct)).ShouldHaveSingleItem().ShouldEndWith("_InitialCreate");
    }

    private static async Task<List<string>> ReadMigrationIdsAsync(string connectionString, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);
        await using var command = new SqlCommand("SELECT MigrationId FROM dbo.__EFMigrationsHistory", connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var ids = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetString(0));
        }

        return ids;
    }
}
