using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Genocs.Saga;

internal sealed class SagaStartupDiagnosticsHostedService(
    SagaRegistrationDiagnostics diagnostics,
    ILoggerFactory loggerFactory) : IHostedService
{
    private readonly SagaRegistrationDiagnostics _diagnostics = diagnostics;
    private readonly ILogger _logger = loggerFactory.CreateLogger<SagaStartupDiagnosticsHostedService>();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Saga startup diagnostics: scanned {AssemblyCount} assemblies, discovered {SagaCount} sagas, state repository {StateRepositoryType}, saga log {SagaLogType}, execution lock {ExecutionLockType}.",
            _diagnostics.ScannedAssemblies.Count,
            _diagnostics.Sagas.Count,
            _diagnostics.StateRepositoryType.FullName,
            _diagnostics.SagaLogType.FullName,
            _diagnostics.ExecutionLockType.FullName);

        if (_diagnostics.Sagas.Count == 0)
        {
            _logger.LogWarning(
                "Saga startup diagnostics: no saga types were discovered. Scanned assemblies: {Assemblies}.",
                string.Join(", ", _diagnostics.ScannedAssemblies.Select(static assembly => assembly.GetName().Name ?? assembly.FullName ?? assembly.ToString())));

            return Task.CompletedTask;
        }

        foreach (SagaTypeRegistrationDiagnostics saga in _diagnostics.Sagas)
        {
            if (saga.Bindings.Count == 0)
            {
                _logger.LogWarning(
                    "Saga startup diagnostics: saga {SagaType} was registered but exposes no ISagaAction<TMessage> bindings.",
                    saga.SagaType.FullName);

                continue;
            }

            _logger.LogInformation(
                "Saga startup diagnostics: saga {SagaType} bindings: {Bindings}.",
                saga.SagaType.FullName,
                string.Join(", ", saga.Bindings.Select(FormatBinding)));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static string FormatBinding(SagaMessageRegistrationDiagnostics binding)
        => binding.StartsSaga
            ? $"{binding.MessageType.FullName} (start)"
            : binding.MessageType.FullName ?? binding.MessageType.Name;
}