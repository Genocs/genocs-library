using System.Collections.Concurrent;
using Genocs.Common.CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Queries.Dispatchers;

/// <summary>
/// Default implementation of the <see cref="IQueryDispatcher"/> interface that uses the built-in
/// dependency injection container to resolve query handlers.
/// </summary>
/// <remarks>
/// The non-generic <see cref="QueryAsync{TResult}"/> overload dispatches through a per-query-type
/// helper that is created once and cached, so the hot path contains no reflection.
/// </remarks>
internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    // one invoker instance per (concrete query type, result type) pair — created once, cached forever
    private static readonly ConcurrentDictionary<(Type Query, Type Result), IQueryHandlerInvoker> _invokerCache = new();

    public QueryDispatcher(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoker = GetOrCreateInvoker(query.GetType(), typeof(TResult));

        await using var scope = _serviceProvider.CreateAsyncScope();
        object? boxed = await invoker.InvokeAsync(scope.ServiceProvider, query, cancellationToken);
        return (TResult?)boxed;
    }

    public async Task<TResult?> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query, cancellationToken);
    }

    private static IQueryHandlerInvoker GetOrCreateInvoker(Type queryType, Type resultType)
        => _invokerCache.GetOrAdd(
            (queryType, resultType),
            static key =>
            {
                var invokerType = typeof(QueryHandlerInvoker<,>).MakeGenericType(key.Query, key.Result);
                return (IQueryHandlerInvoker)Activator.CreateInstance(invokerType)!;
            });

    // -------------------------------------------------------------------------
    // Internal helpers — not part of the public API
    // -------------------------------------------------------------------------

    private interface IQueryHandlerInvoker
    {
        Task<object?> InvokeAsync(IServiceProvider sp, object query, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Typed invoker that closes over the concrete TQuery/TResult parameters so handler resolution
    /// and invocation are fully generic (no MethodInfo.Invoke on the hot path).
    /// </summary>
    private sealed class QueryHandlerInvoker<TQuery, TResult> : IQueryHandlerInvoker
        where TQuery : class, IQuery<TResult>
    {
        public async Task<object?> InvokeAsync(IServiceProvider sp, object query, CancellationToken cancellationToken)
        {
            var handler = sp.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync((TQuery)query, cancellationToken);
        }
    }
}
