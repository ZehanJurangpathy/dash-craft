namespace DashCraft.Domain.Tests;

/// <summary>Constitution, Backend structure: Domain has no dependencies on other layers.</summary>
public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_does_not_reference_other_DashCraft_assemblies()
    {
        var references = typeof(DomainAssembly).Assembly.GetReferencedAssemblies().Select(a => a.Name);

        references.ShouldNotContain(name => name != null && name.StartsWith("DashCraft.", StringComparison.Ordinal));
    }
}
