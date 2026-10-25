using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DashCraft.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers Application services. Mediator itself is registered by the API host, because its
    /// source generator runs in the composing project.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly, includeInternalTypes: true);
        return services;
    }
}
