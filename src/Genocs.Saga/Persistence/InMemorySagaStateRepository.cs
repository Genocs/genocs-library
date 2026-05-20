using System.Collections.Concurrent;

namespace Genocs.Saga.Persistence;

internal class InMemorySagaStateRepository : ISagaStateRepository
{
    private readonly ConcurrentDictionary<string, SagaState> _repository;

    public InMemorySagaStateRepository() => _repository = new();

    public Task<ISagaState?> ReadAsync(SagaId id, Type type)
        => Task.FromResult(
            _repository.TryGetValue(GetKey(id, type), out SagaState? state)
                ? (ISagaState?)SagaState.CopyOf(state)
                : null);

    public Task WriteAsync(ISagaState state)
    {
        if (state.Id is null)
        {
            throw new SagaException("Saga state id must be provided.");
        }

        if (state.Type is null)
        {
            throw new SagaException("Saga state type must be provided.");
        }

        string key = GetKey(state.Id.Value, state.Type);

        while (true)
        {
            if (_repository.TryGetValue(key, out SagaState? current))
            {
                if (current.Version != state.Version)
                {
                    throw new SagaConcurrencyException($"Stale saga state write detected for '{state.Type.FullName}' with id '{state.Id.Value.Id}'. Expected version {current.Version}, but received {state.Version}.");
                }

                SagaState updated = SagaState.Create(state.Id.Value, state.Type, state.State, state.Data, state.Version + 1);

                if (_repository.TryUpdate(key, updated, current))
                {
                    state.UpdateVersion(updated.Version);
                    return Task.CompletedTask;
                }

                continue;
            }

            if (state.Version != 0)
            {
                throw new SagaConcurrencyException($"Cannot create saga state for '{state.Type.FullName}' with id '{state.Id.Value.Id}' using version {state.Version}. New saga state instances must start at version 0.");
            }

            SagaState created = SagaState.Create(state.Id.Value, state.Type, state.State, state.Data, version: 1);

            if (_repository.TryAdd(key, created))
            {
                state.UpdateVersion(created.Version);
                return Task.CompletedTask;
            }
        }

    }

    private static string GetKey(SagaId id, Type type)
        => $"{id.Id}:{type.AssemblyQualifiedName}";
}
