using Genocs.Saga.Persistence;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Persistence;

public class InMemoryPersistenceTests
{
    [Fact]
    public async Task InMemorySagaStateRepository_WhenWritingMultipleSagaIdsConcurrently_ShouldRetainAllEntries()
    {
        InMemorySagaStateRepository repository = new();

        Task[] writes = Enumerable.Range(0, 100)
            .Select(index => repository.WriteAsync(SagaState.Create($"state-{index}", typeof(TestSaga), SagaProcessState.Pending)))
            .ToArray();

        await Task.WhenAll(writes);

        for (int index = 0; index < 100; index++)
        {
            ISagaState? state = await repository.ReadAsync($"state-{index}", typeof(TestSaga));
            state.ShouldNotBeNull();
            state.Id.ShouldNotBeNull();
            state.Id.Value.Id.ShouldBe($"state-{index}");
            state.Version.ShouldBe(1);
        }
    }

    [Fact]
    public async Task InMemorySagaStateRepository_WhenUpdatingCurrentState_ShouldIncrementVersion()
    {
        InMemorySagaStateRepository repository = new();
        SagaState state = SagaState.Create("state-1", typeof(TestSaga), SagaProcessState.Pending);

        await repository.WriteAsync(state);
        state.Version.ShouldBe(1);

        state.Update(SagaProcessState.Completed);

        await repository.WriteAsync(state);

        state.Version.ShouldBe(2);

        ISagaState? persisted = await repository.ReadAsync("state-1", typeof(TestSaga));
        persisted.ShouldNotBeNull();
        persisted.State.ShouldBe(SagaProcessState.Completed);
        persisted.Version.ShouldBe(2);
    }

    [Fact]
    public async Task InMemorySagaStateRepository_WhenWritingStaleStateSnapshot_ShouldThrowSagaConcurrencyException()
    {
        InMemorySagaStateRepository repository = new();
        SagaState state = SagaState.Create("state-1", typeof(TestSaga), SagaProcessState.Pending);

        await repository.WriteAsync(state);

        ISagaState? firstSnapshot = await repository.ReadAsync("state-1", typeof(TestSaga));
        ISagaState? staleSnapshot = await repository.ReadAsync("state-1", typeof(TestSaga));

        firstSnapshot.ShouldNotBeNull();
        staleSnapshot.ShouldNotBeNull();

        firstSnapshot.Update(SagaProcessState.Completed);
        await repository.WriteAsync(firstSnapshot);

        staleSnapshot.Update(SagaProcessState.Rejected);

        await Should.ThrowAsync<SagaConcurrencyException>(() => repository.WriteAsync(staleSnapshot));
    }

    [Fact]
    public async Task InMemorySagaLog_WhenWritingMultipleSagaIdsConcurrently_ShouldRetainAllEntries()
    {
        InMemorySagaLog log = new();

        Task[] writes = Enumerable.Range(0, 100)
            .Select(index => log.WriteAsync(SagaLogData.Create($"log-{index}", typeof(TestSaga), new TestMessage(index), SagaLogEntryOutcome.Completed)))
            .ToArray();

        await Task.WhenAll(writes);

        for (int index = 0; index < 100; index++)
        {
            List<ISagaLogData> entries = (await log.ReadAsync($"log-{index}", typeof(TestSaga))).ToList();
            entries.Count.ShouldBe(1);
            entries[0].Message.ShouldBeOfType<TestMessage>();
            ((TestMessage)entries[0].Message).Value.ShouldBe(index);
        }
    }

    [Fact]
    public async Task InMemorySagaLog_WhenUpdatingOutcome_ShouldPersistUpdatedStatus()
    {
        InMemorySagaLog log = new();
        SagaLogData entry = SagaLogData.Create("log-1", typeof(TestSaga), new TestMessage(42), SagaLogEntryOutcome.Completed, "msg-42");

        await log.WriteAsync(entry);
        await log.UpdateOutcomeAsync("log-1", typeof(TestSaga), entry.EntryId, SagaLogEntryOutcome.Compensated);

        ISagaLogData persisted = (await log.ReadAsync("log-1", typeof(TestSaga))).Single();
        persisted.Outcome.ShouldBe(SagaLogEntryOutcome.Compensated);
        persisted.MessageId.ShouldBe("msg-42");
    }

    private sealed class TestSaga : Saga;

    private sealed record TestMessage(int Value);
}