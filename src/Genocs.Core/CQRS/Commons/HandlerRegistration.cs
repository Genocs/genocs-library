using System.Reflection;
using Genocs.Common.Types;
using Genocs.Core.Builders;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace Genocs.Core.CQRS.Commons;

internal static class HandlerRegistration
{
    internal static Assembly[] GetCandidateAssemblies(string? project = null)
    {
        IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.FullName));

        if (!string.IsNullOrWhiteSpace(project))
        {
            assemblies = assemblies.Where(a => a.FullName!.Contains(project, StringComparison.OrdinalIgnoreCase));
        }

        return assemblies
            .DistinctBy(a => a.FullName, StringComparer.Ordinal)
            .OrderBy(a => a.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IServiceCollection AddHandlerRegistrations(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Type handlerInterfaceType,
        ServiceLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentNullException.ThrowIfNull(handlerInterfaceType);

        var candidateAssemblies = assemblies.ToArray();
        if (candidateAssemblies.Length == 0)
        {
            CoreDiagnosticsRuntime.Info(services, $"No assemblies discovered for handler interface '{handlerInterfaceType.Name}'.");
            return services;
        }

        int discoveredHandlerTypes = candidateAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsAbstract: false, IsInterface: false }
                && type.GetInterfaces().Any(@interface =>
                    @interface.IsGenericType
                    && @interface.GetGenericTypeDefinition() == handlerInterfaceType))
            .Distinct()
            .Count();

        CoreDiagnosticsRuntime.Info(
            services,
            $"Discovered {discoveredHandlerTypes} candidate handler type(s) for '{handlerInterfaceType.Name}' across {candidateAssemblies.Length} assembly(ies).");

        services.Scan(scan =>
        {
            var registration = scan.FromAssemblies(candidateAssemblies)
                .AddClasses(c => c.AssignableTo(handlerInterfaceType)
                    .WithoutAttribute<DecoratorAttribute>())
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces();

            _ = lifetime switch
            {
                ServiceLifetime.Singleton => registration.WithSingletonLifetime(),
                ServiceLifetime.Scoped => registration.WithScopedLifetime(),
                _ => registration.WithTransientLifetime(),
            };
        });

        return services;
    }
}