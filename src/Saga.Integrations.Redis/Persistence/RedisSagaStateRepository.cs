using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Genocs.Saga.Integrations.Redis.Persistence;

internal sealed class RedisSagaStateRepository(IRedisSagaStateStore stateStore) : ISagaStateRepository
{
    private readonly IRedisSagaStateStore _stateStore = stateStore;

    public async Task<ISagaState?> ReadAsync(SagaId sagaId, Type sagaType)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
        {
            throw new SagaException($"{nameof(sagaId)} was null or whitespace.");
        }

        if (sagaType is null)
        {
            throw new SagaException($"{nameof(sagaType)} was null.");
        }

        RedisSagaState? state = null;
        string? cachedSagaState = await _stateStore.GetStringAsync(StateId(sagaId, sagaType));

        if (!string.IsNullOrWhiteSpace(cachedSagaState))
        {
            state = JsonConvert.DeserializeObject<RedisSagaState>(cachedSagaState);
            state?.Update(state.State, (state.Data as JObject)?.ToObject(state.DataType));
        }

        return state;
    }

    public async Task WriteAsync(ISagaState state)
    {
        if (state is null)
        {
            throw new SagaException($"{nameof(state)} was null.");
        }

        if (state.Id is null)
        {
            throw new SagaException("Saga state id must be provided.");
        }

        if (state.Type is null)
        {
            throw new SagaException("Saga state type must be provided.");
        }

        string key = StateId(state.Id, state.Type);
        string? currentSerialized = await _stateStore.GetStringAsync(key);
        RedisSagaState? current = DeserializeState(currentSerialized);

        if (current is null)
        {
            if (state.Version != 0)
            {
                throw new SagaConcurrencyException($"Cannot create saga state for '{state.Type.FullName}' with id '{state.Id.Value.Id}' using version {state.Version}. New saga state instances must start at version 0.");
            }
        }
        else if (current.Version != state.Version)
        {
            throw new SagaConcurrencyException($"Stale saga state write detected for '{state.Type.FullName}' with id '{state.Id.Value.Id}'. Expected version {current.Version}, but received {state.Version}.");
        }

        long nextVersion = state.Version + 1;
        var sagaState = new RedisSagaState(state.Id.Value, state.Type, state.State, state.Data, nextVersion, state.Data?.GetType());

        string serializedSagaState = JsonConvert.SerializeObject(sagaState);

        if (!await _stateStore.CompareAndSetAsync(key, currentSerialized, serializedSagaState))
        {
            RedisSagaState? latest = DeserializeState(await _stateStore.GetStringAsync(key));
            long? latestVersion = latest?.Version;
            throw new SagaConcurrencyException($"Stale saga state write detected for '{state.Type.FullName}' with id '{state.Id.Value.Id}'. Expected version {state.Version}, but the stored version is {latestVersion?.ToString() ?? "missing"}." );
        }

        state.UpdateVersion(nextVersion);
    }

    public async Task DeleteAsync(SagaId sagaId, Type sagaType)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
        {
            throw new SagaException($"{nameof(sagaId)} was null or whitespace.");
        }

        if (sagaType is null)
        {
            throw new SagaException($"{nameof(sagaType)} was null.");
        }

        await _stateStore.RemoveAsync(StateId(sagaId, sagaType));
    }

    private string StateId(string id, Type type) => $"_state_{id}_{type.GetHashCode()}";

    private static RedisSagaState? DeserializeState(string? serializedState)
    {
        if (string.IsNullOrWhiteSpace(serializedState))
        {
            return null;
        }

        RedisSagaState? state = JsonConvert.DeserializeObject<RedisSagaState>(serializedState);
        state?.Update(state.State, (state.Data as JObject)?.ToObject(state.DataType));
        return state;
    }
}