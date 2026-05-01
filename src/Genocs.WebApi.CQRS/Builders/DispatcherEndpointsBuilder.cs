using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.WebApi.CQRS.Builders;

public class DispatcherEndpointsBuilder(IEndpointsBuilder builder) : IDispatcherEndpointsBuilder
{
    private readonly IEndpointsBuilder _builder = builder;

    public IDispatcherEndpointsBuilder Get(string path, Func<HttpContext, CancellationToken, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
    {
        _builder.Get(
            path,
            context is null ? null : (ctx =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS endpoints require a non-null HttpContext.");
                }

                return context(ctx, ctx.RequestAborted);
            }),
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Get<TQuery, TResult>(
        string path,
        Func<TQuery, HttpContext, CancellationToken, Task>? beforeDispatch = null,
        Func<TQuery, TResult?, HttpContext, CancellationToken, Task>? afterDispatch = null,
        Action<IEndpointConventionBuilder>? endpoint = null,
        bool auth = false,
        string? roles = null,
        params string[] policies)
        where TQuery : class, IQuery<TResult>
    {
        _builder.Get<TQuery, TResult>(
            path,
            async (query, ctx) =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS query endpoints require a non-null HttpContext.");
                }

                var cancellationToken = ctx.RequestAborted;

                if (beforeDispatch is not null)
                {
                    await beforeDispatch(query, ctx, cancellationToken);
                }

                var dispatcher = ctx.RequestServices.GetRequiredService<IQueryDispatcher>();
                var result = await dispatcher.QueryAsync<TQuery, TResult>(query, cancellationToken);
                if (afterDispatch is null)
                {
                    if (result is null)
                    {
                        ctx.Response.StatusCode = 404;
                        return;
                    }

                    await ctx.Response.WriteJsonAsync(result);
                    return;
                }

                await afterDispatch(query, result, ctx, cancellationToken);
            },
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Post(
                                            string path,
                                            Func<HttpContext, CancellationToken, Task>? context = null,
                                            Action<IEndpointConventionBuilder>? endpoint = null,
                                            bool auth = false,
                                            string? roles = null,
                                            params string[] policies)
    {
        _builder.Post(
            path,
            context is null ? null : (ctx =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS endpoints require a non-null HttpContext.");
                }

                return context(ctx, ctx.RequestAborted);
            }),
            endpoint,
            auth,
            roles,
            policies);
        return this;
    }

    public IDispatcherEndpointsBuilder Post<T>(
                                                string path,
                                                Func<T, HttpContext, CancellationToken, Task>? beforeDispatch = null,
                                                Func<T, HttpContext, CancellationToken, Task>? afterDispatch = null,
                                                Action<IEndpointConventionBuilder>? endpoint = null,
                                                bool auth = false,
                                                string? roles = null,
                                                params string[] policies)
        where T : class, ICommand
    {
        _builder.Post<T>(
            path,
            (cmd, ctx) =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS command endpoints require a non-null HttpContext.");
                }

                return BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch);
            },
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Put(string path, Func<HttpContext, CancellationToken, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
    {
        _builder.Put(
            path,
            context is null ? null : (ctx =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS endpoints require a non-null HttpContext.");
                }

                return context(ctx, ctx.RequestAborted);
            }),
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Put<T>(
                                                string path,
                                                Func<T, HttpContext, CancellationToken, Task>? beforeDispatch = null,
                                                Func<T, HttpContext, CancellationToken, Task>? afterDispatch = null,
                                                Action<IEndpointConventionBuilder>? endpoint = null,
                                                bool auth = false,
                                                string? roles = null,
                                                params string[] policies)
        where T : class, ICommand
    {
        _builder.Put<T>(
            path,
            (cmd, ctx) =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS command endpoints require a non-null HttpContext.");
                }

                return BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch);
            },
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Delete(
        string path,
        Func<HttpContext, CancellationToken, Task>? context = null,
        Action<IEndpointConventionBuilder>? endpoint = null,
        bool auth = false,
        string? roles = null,
        params string[] policies)
    {
        _builder.Delete(
            path,
            context is null ? null : (ctx =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS endpoints require a non-null HttpContext.");
                }

                return context(ctx, ctx.RequestAborted);
            }),
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    public IDispatcherEndpointsBuilder Delete<T>(
        string path,
        Func<T, HttpContext, CancellationToken, Task>? beforeDispatch = null,
        Func<T, HttpContext, CancellationToken, Task>? afterDispatch = null,
        Action<IEndpointConventionBuilder>? endpoint = null,
        bool auth = false,
        string? roles = null,
        params string[] policies)
        where T : class, ICommand
    {
        _builder.Delete<T>(
            path,
            (cmd, ctx) =>
            {
                if (ctx is null)
                {
                    throw new InvalidOperationException("CQRS command endpoints require a non-null HttpContext.");
                }

                return BuildCommandContext(cmd, ctx, beforeDispatch, afterDispatch);
            },
            endpoint,
            auth,
            roles,
            policies);

        return this;
    }

    private static async Task BuildCommandContext<T>(
        T command,
        HttpContext context,
        Func<T, HttpContext, CancellationToken, Task>? beforeDispatch = null,
        Func<T, HttpContext, CancellationToken, Task>? afterDispatch = null)
        where T : class, ICommand
    {
        var cancellationToken = context.RequestAborted;

        if (beforeDispatch is not null)
        {
            await beforeDispatch(command, context, cancellationToken);
        }

        ICommandDispatcher dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();
        await dispatcher.SendAsync(command, cancellationToken);

        context.Response.StatusCode = 200;

        if (afterDispatch is not null)
        {
            await afterDispatch(command, context, cancellationToken);
        }
    }
}