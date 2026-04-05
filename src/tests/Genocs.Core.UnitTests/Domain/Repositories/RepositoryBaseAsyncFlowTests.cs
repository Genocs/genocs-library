using System.Linq.Expressions;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Xunit;

namespace Genocs.Core.UnitTests.Domain.Repositories;

public class RepositoryBaseAsyncFlowTests
{
    [Fact]
    public async Task FirstOrDefaultAsync_UsesAsyncCoreHook()
    {
        var expected = new TestEntity { Id = Guid.NewGuid() };
        var repository = new AsyncOnlyRepository(expectedEntity: expected);

        TestEntity? result = await repository.FirstOrDefaultAsync(expected.Id);

        Assert.True(repository.FirstOrDefaultByIdCoreAsyncCalled);
        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetAllListAsync_UsesAsyncCoreHook()
    {
        var expected = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() },
        };
        var repository = new AsyncOnlyRepository(listResult: expected);

        List<TestEntity> result = await repository.GetAllListAsync();

        Assert.True(repository.GetAllListCoreAsyncCalled);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CountAsync_UsesAsyncCoreHook()
    {
        var repository = new AsyncOnlyRepository(countResult: 7);

        int result = await repository.CountAsync();

        Assert.True(repository.CountCoreAsyncCalled);
        Assert.Equal(7, result);
    }

    private sealed class AsyncOnlyRepository : RepositoryBase<TestEntity, Guid>
    {
        private readonly TestEntity? _expectedEntity;
        private readonly List<TestEntity>? _listResult;
        private readonly int _countResult;

        public AsyncOnlyRepository(TestEntity? expectedEntity = null, List<TestEntity>? listResult = null, int countResult = 0)
        {
            _expectedEntity = expectedEntity;
            _listResult = listResult;
            _countResult = countResult;
        }

        public bool FirstOrDefaultByIdCoreAsyncCalled { get; private set; }

        public bool GetAllListCoreAsyncCalled { get; private set; }

        public bool CountCoreAsyncCalled { get; private set; }

        public override IQueryable<TestEntity> GetAll() => throw new InvalidOperationException("Sync query path should not be used in async-first tests.");

        public override TestEntity Insert(TestEntity entity) => entity;

        public override TestEntity Update(TestEntity entity) => entity;

        public override void Delete(TestEntity entity)
        {
        }

        public override void Delete(Guid id)
        {
        }

        protected override Task<TestEntity?> FirstOrDefaultByIdCoreAsync(Guid id, CancellationToken cancellationToken = default)
        {
            FirstOrDefaultByIdCoreAsyncCalled = true;
            return Task.FromResult(_expectedEntity);
        }

        protected override Task<List<TestEntity>> GetAllListCoreAsync(CancellationToken cancellationToken = default)
        {
            GetAllListCoreAsyncCalled = true;
            return Task.FromResult(_listResult ?? []);
        }

        protected override Task<int> CountCoreAsync(CancellationToken cancellationToken = default)
        {
            CountCoreAsyncCalled = true;
            return Task.FromResult(_countResult);
        }
    }

    private sealed class TestEntity : Entity<Guid>;
}
