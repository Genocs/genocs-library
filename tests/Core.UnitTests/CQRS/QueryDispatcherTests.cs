using Genocs.Common.CQRS.Commons;
using Genocs.Common.CQRS.Queries;
using Genocs.Core.CQRS.Commons;
using Genocs.Core.CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.CQRS;

public class QueryDispatcherTests
{
    // -------------------------------------------------------------------------
    // Typed dispatch: QueryAsync<TQuery, TResult>
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TypedDispatch_ReturnsHandlerResult()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();

        int result = await dispatcher.QueryAsync<PingQuery, int>(new PingQuery(42));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task TypedDispatch_ThrowsOnNullQuery()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dispatcher.QueryAsync<PingQuery, int>(null!));
    }

    // -------------------------------------------------------------------------
    // Polymorphic dispatch: QueryAsync<TResult>(IQuery<TResult>)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PolymorphicDispatch_ReturnsHandlerResult()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();

        IQuery<int> query = new PingQuery(7);
        int? result = await dispatcher.QueryAsync(query);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task PolymorphicDispatch_ThrowsOnNullQuery()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => dispatcher.QueryAsync<int>(null!));
    }

    [Fact]
    public async Task PolymorphicDispatch_ThrowsWhenHandlerNotRegistered()
    {
        ServiceProvider sp = BuildProvider(includeHandler: false);
        var dispatcher = sp.GetRequiredService<IQueryDispatcher>();

        IQuery<int> query = new PingQuery(1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.QueryAsync(query));
    }

    // -------------------------------------------------------------------------
    // IDispatcher facade delegates to the same paths
    // -------------------------------------------------------------------------

    [Fact]
    public async Task InMemoryDispatcher_PolymorphicDispatch_ReturnsResult()
    {
        ServiceProvider sp = BuildProvider();
        var dispatcher = sp.GetRequiredService<IDispatcher>();

        IQuery<int> query = new PingQuery(99);
        int? result = await dispatcher.QueryAsync(query);

        Assert.Equal(99, result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ServiceProvider BuildProvider(bool includeHandler = true)
    {
        var services = new ServiceCollection();

        services.AddDispatchers();

        if (includeHandler)
        {
            services.AddTransient<IQueryHandler<PingQuery, int>, PingQueryHandler>();
        }

        return services.BuildServiceProvider();
    }

    private sealed record PingQuery(int Value) : IQuery<int>;

    private sealed class PingQueryHandler : IQueryHandler<PingQuery, int>
    {
        public Task<int> HandleAsync(PingQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(query.Value);
    }
}
