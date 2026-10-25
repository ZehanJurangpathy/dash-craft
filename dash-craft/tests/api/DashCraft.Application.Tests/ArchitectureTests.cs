namespace DashCraft.Application.Tests;

/// <summary>Constitution, Backend structure: Application depends only on Domain, never on infrastructure.</summary>
public sealed class ArchitectureTests
{
    private static readonly string[] References =
        [.. typeof(ApplicationAssembly).Assembly.GetReferencedAssemblies().Select(a => a.Name ?? "")];

    [Fact]
    public void Application_does_not_reference_Infrastructure_or_Api()
    {
        References.ShouldNotContain("DashCraft.Infrastructure");
        References.ShouldNotContain("DashCraft.Api");
    }

    [Fact]
    public void Application_does_not_reference_data_access_libraries()
    {
        References.ShouldNotContain(name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        References.ShouldNotContain("Dapper");
        References.ShouldNotContain("Microsoft.Data.SqlClient");
    }
}
