using System.Reflection;

namespace Genocs.Saga;

public sealed record SagaRegistrationDiagnostics(
    IReadOnlyList<Assembly> ScannedAssemblies,
    IReadOnlyList<SagaTypeRegistrationDiagnostics> Sagas,
    Type StateRepositoryType,
    Type SagaLogType,
    Type ExecutionLockType);

public sealed record SagaTypeRegistrationDiagnostics(
    Type SagaType,
    IReadOnlyList<SagaMessageRegistrationDiagnostics> Bindings);

public sealed record SagaMessageRegistrationDiagnostics(
    Type MessageType,
    bool StartsSaga);