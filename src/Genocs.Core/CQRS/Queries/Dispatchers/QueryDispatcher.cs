using Genocs.Common.CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.CQRS.Queries.Dispatchers;

/// <summary>
/// Default implementation of the <see cref="IQueryDispatcher"/> interface that uses the built-in dependency injection container to resolve query handlers.
/// </summary>
internal sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            throw new InvalidOperationException("Query cannot be null.");
        }

        await using var scope = _serviceProvider.CreateAsyncScope();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        object handler = scope.ServiceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync)) ?? throw new InvalidOperationException($"Query handler for '{typeof(TResult).Name}' is invalid.");

        return await (Task<TResult?>)method?.Invoke(handler, new object[] { query, cancellationToken });
    }

    public async Task<TResult?> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : class, IQuery<TResult>
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query, cancellationToken);
    }
}
