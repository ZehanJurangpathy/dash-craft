namespace DashCraft.Infrastructure.Tests;

/// <summary>Constitution, Backend structure: Infrastructure implements Application abstractions and has no web dependency.</summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Infrastructure_does_not_reference_the_web_layer()
    {
        var references = typeof(InfrastructureAssembly).Assembly.GetReferencedAssemblies().Select(a => a.Name ?? "").ToList();

        references.ShouldNotContain("DashCraft.Api");
        references.ShouldNotContain(name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }
}
