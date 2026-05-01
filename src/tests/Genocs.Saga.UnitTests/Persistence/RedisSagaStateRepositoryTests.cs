using Genocs.Saga.Integrations.Redis.Persistence;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Persistence;

public class RedisSagaStateRepositoryTests
{
    [Fact]
    public async Task WriteAsync_WhenCreatingNewSagaState_ShouldWriteWithAtomicCreateAndIncrementVersion()
    {
        FakeRedisSagaStateStore stateStore = new();
        RedisSagaStateRepository repository = new(stateStore);
        RedisSagaState state = new("saga-1", typeof(TestSaga), SagaProcessState.Pending, new TestData("created"));

        await repository.WriteAsync(state);

        state.Version.ShouldBe(1);

        ISagaState? persisted = await repository.ReadAsync("saga-1", typeof(TestSaga));
        persisted.ShouldNotBeNull();
        persisted.Id.ShouldNotBeNull();
        persisted.Id.Value.Id.ShouldBe("saga-1");
        persisted.Version.ShouldBe(1);
        persisted.State.ShouldBe(SagaProcessState.Pending);
    }

    [Fact]
    public async Task WriteAsync_WhenStoredVersionIsAlreadyNewer_ShouldThrowSagaConcurrencyException()
    {
        FakeRedisSagaStateStore stateStore = new();
        RedisSagaStateRepository repository = new(stateStore);

        stateStore.Seed("_state_saga-1_" + typeof(TestSaga).GetHashCode(), Serialize(new RedisSagaState("saga-1", typeof(TestSaga), SagaProcessState.Completed, new TestData("persisted"), 2, typeof(TestData))));

        RedisSagaState staleState = new("saga-1", typeof(TestSaga), SagaProcessState.Rejected, new TestData("stale"), 1, typeof(TestData));

        await Should.ThrowAsync<SagaConcurrencyException>(() => repository.WriteAsync(staleState));
    }

    [Fact]
    public async Task WriteAsync_WhenCompareAndSetFailsAfterRead_ShouldThrowSagaConcurrencyException()
    {
        FakeRedisSagaStateStore stateStore = new();
        RedisSagaStateRepository repository = new(stateStore);

        string key = "_state_saga-1_" + typeof(TestSaga).GetHashCode();
        stateStore.Seed(key, Serialize(new RedisSagaState("saga-1", typeof(TestSaga), SagaProcessState.Pending, new TestData("persisted"), 1, typeof(TestData))));
        stateStore.FailNextCompareAndSetWithReplacement(Serialize(new RedisSagaState("saga-1", typeof(TestSaga), SagaProcessState.Completed, new TestData("other-writer"), 2, typeof(TestData))));

        RedisSagaState currentState = new("saga-1", typeof(TestSaga), SagaProcessState.Rejected, new TestData("candidate"), 1, typeof(TestData));

        SagaConcurrencyException exception = await Should.ThrowAsync<SagaConcurrencyException>(() => repository.WriteAsync(currentState));

        exception.Message.ShouldContain("stored version is 2");
        currentState.Version.ShouldBe(1);
    }

    [Fact]
    public async Task DeserializeRedisSagaState_ShouldDeserializeCorrectly()
    {
        string cachedSagaState = @"{""Id"":{""Id"":""1e7badd1-350b-4d52-9e14-61bd0ac14b57""},""Type"":""Genocs.Library.Demo.WebApi.Sagas.SampleSaga, Genocs.Library.Demo.WebApi, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null"",""State"":0,""Data"":{""IsStartTransaction"":true,""IsCompleteTransaction"":false,""TransactionValue"":10,""IsEnded"":false,""MessageId"":""3d2b9216-1d9c-442d-93c0-c06c1049a452"",""IsSagaCompleted"":false},""Version"":1,""DataType"":""Genocs.Library.Demo.WebApi.Sagas.SagaData, Genocs.Library.Demo.WebApi, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null""}";
        RedisSagaState? state = JsonConvert.DeserializeObject<RedisSagaState>(cachedSagaState);

        state.ShouldNotBeNull();
        state.Id!.Value.Id.ShouldBe("1e7badd1-350b-4d52-9e14-61bd0ac14b57");
    }

    private static string Serialize(RedisSagaState state)
        => JsonConvert.SerializeObject(state);

    private sealed class TestSaga : Saga;

    private sealed record TestData(string Value);

    private sealed class FakeRedisSagaStateStore : IRedisSagaStateStore
    {
        private readonly Dictionary<string, string> _values = new();
        private string? _replacementValueOnNextFailure;
        private bool _failNextCompareAndSet;

        public Task<string?> GetStringAsync(string key)
            => Task.FromResult(_values.TryGetValue(key, out string? value) ? value : null);

        public Task<bool> CompareAndSetAsync(string key, string expectedValue, string newValue)
        {
            if (_failNextCompareAndSet)
            {
                _failNextCompareAndSet = false;

                if (_replacementValueOnNextFailure is not null)
                {
                    _values[key] = _replacementValueOnNextFailure;
                    _replacementValueOnNextFailure = null;
                }

                return Task.FromResult(false);
            }

            bool hasCurrent = _values.TryGetValue(key, out string currentValue);

            if (expectedValue is null)
            {
                if (hasCurrent)
                {
                    return Task.FromResult(false);
                }
            }
            else if (!hasCurrent || !string.Equals(currentValue, expectedValue, StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            _values[key] = newValue;
            return Task.FromResult(true);
        }

        public Task RemoveAsync(string key)
        {
            _values.Remove(key);
            return Task.CompletedTask;
        }

        public void Seed(string key, string value)
            => _values[key] = value;

        public void FailNextCompareAndSetWithReplacement(string replacementValue)
        {
            _failNextCompareAndSet = true;
            _replacementValueOnNextFailure = replacementValue;
        }
    }
}