using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DashCraft.IntegrationTests.Infrastructure;

/// <summary>Runs the real API in-process against the given application database.</summary>
public sealed class DashCraftApiFactory(string connectionString, bool applyMigrations = true, int startupMaxRetries = 1)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting (not ConfigureAppConfiguration) so the values are visible while Program.cs
        // registers services.
        builder.UseSetting("ConnectionStrings:DashCraft", connectionString);
        builder.UseSetting("Database:ApplyMigrationsOnStartup", applyMigrations.ToString(CultureInfo.InvariantCulture));
        builder.UseSetting("Database:StartupMaxRetries", startupMaxRetries.ToString(CultureInfo.InvariantCulture));
        builder.UseSetting("Database:StartupRetryDelay", "00:00:01");
    }

    /// <summary>
    /// Polls GET /health until it returns 200. Migrations run in the background, so the API is not
    /// necessarily ready right after it starts.
    /// </summary>
    public async Task<HttpResponseMessage> WaitUntilHealthyAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(60));
        var lastBody = "";

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync("/health", cancellationToken);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response;
            }

            lastBody = await response.Content.ReadAsStringAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
        }

        throw new TimeoutException($"The API did not become healthy in time. Last /health response: {lastBody}");
    }
}
