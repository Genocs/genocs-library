using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Genocs.Core.Builders;

public sealed class CoreDiagnosticsOptions
{
    public bool Enabled { get; set; }

    public bool WarnOnEmptyHandlerSet { get; set; } = true;
}

public sealed class CoreDiagnosticsState
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    internal void AddInfo(string message)
        => _messages.Add($"INFO: {message}");

    internal void AddWarning(string message)
        => _messages.Add($"WARN: {message}");
}

public static class CoreDiagnosticsExtensions
{
    public static IGenocsBuilder AddCoreDiagnostics(this IGenocsBuilder builder, Action<CoreDiagnosticsOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddCoreDiagnostics(configure);
        return builder;
    }

    public static IServiceCollection AddCoreDiagnostics(this IServiceCollection services, Action<CoreDiagnosticsOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new CoreDiagnosticsOptions
        {
            Enabled = true,
            WarnOnEmptyHandlerSet = true
        };
        var state = new CoreDiagnosticsState();

        configure?.Invoke(options);

        services.RemoveAll<CoreDiagnosticsOptions>();
        services.RemoveAll<CoreDiagnosticsState>();
        services.AddSingleton(options);
        services.AddSingleton(state);

        return services;
    }
}

internal static class CoreDiagnosticsRuntime
{
    internal static bool TryGetEnabledState(IServiceCollection services, out CoreDiagnosticsOptions options, out CoreDiagnosticsState state)
    {
        options = services
            .LastOrDefault(descriptor => descriptor.ServiceType == typeof(CoreDiagnosticsOptions))
            ?.ImplementationInstance as CoreDiagnosticsOptions;

        state = services
            .LastOrDefault(descriptor => descriptor.ServiceType == typeof(CoreDiagnosticsState))
            ?.ImplementationInstance as CoreDiagnosticsState;

        return options?.Enabled == true && state is not null;
    }

    internal static bool TryGetEnabledState(IServiceProvider serviceProvider, out CoreDiagnosticsOptions options, out CoreDiagnosticsState state)
    {
        options = serviceProvider.GetService<CoreDiagnosticsOptions>();
        state = serviceProvider.GetService<CoreDiagnosticsState>();
        return options?.Enabled == true && state is not null;
    }

    internal static void Info(IServiceCollection services, string message)
    {
        if (!TryGetEnabledState(services, out _, out CoreDiagnosticsState state) || state is null)
        {
            return;
        }

        state.AddInfo(message);
    }

    internal static void Warn(IServiceCollection services, string message)
    {
        if (!TryGetEnabledState(services, out _, out CoreDiagnosticsState state) || state is null)
        {
            return;
        }

        state.AddWarning(message);
    }

    internal static void Info(IServiceProvider serviceProvider, string message)
    {
        if (!TryGetEnabledState(serviceProvider, out _, out CoreDiagnosticsState state) || state is null)
        {
            return;
        }

        state.AddInfo(message);
    }
}
