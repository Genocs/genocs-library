using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Genocs.Common.Types;
using Genocs.Core.Builders;
using Genocs.WebApi.CQRS.Builders;
using Genocs.WebApi.CQRS.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.WebApi.CQRS;

public static class Extensions
{
    public static IGenocsBuilder AddInMemoryDispatcher(this IGenocsBuilder builder)
    {
        builder.Services.AddSingleton<IDispatcher, InMemoryDispatcher>();
        return builder;
    }

    public static IEndpointRouteBuilder MapDispatcherEndpoints(
        this IEndpointRouteBuilder routeBuilder,
        Action<IDispatcherEndpointsBuilder> builder)
    {
        var definitions = routeBuilder.ServiceProvider.GetRequiredService<WebApiEndpointDefinitions>();
        builder(new DispatcherEndpointsBuilder(new EndpointsBuilder(routeBuilder, definitions)));

        return routeBuilder;
    }

    public static IEndpointRouteBuilder MapDispatcherEndpoints(
        this IEndpointRouteBuilder routeBuilder,
        Func<IDispatcherEndpointsBuilder, IDispatcherEndpointsBuilder> builder)
    {
        var definitions = routeBuilder.ServiceProvider.GetRequiredService<WebApiEndpointDefinitions>();
        _ = builder(new DispatcherEndpointsBuilder(new EndpointsBuilder(routeBuilder, definitions)));

        return routeBuilder;
    }

    [Obsolete("Use MapDispatcherEndpoints(...) on IEndpointRouteBuilder. This API no longer configures UseRouting, UseAuthorization, or UseEndpoints for the host pipeline.")]
    public static IApplicationBuilder UseDispatcherEndpoints(
                                                                this IApplicationBuilder app,
                                                                Action<IDispatcherEndpointsBuilder> builder,
                                                                bool useAuthorization = true,
                                                                Action<IApplicationBuilder>? middleware = null)
    {
        _ = useAuthorization;
        middleware?.Invoke(app);

        if (app is not IEndpointRouteBuilder routeBuilder)
        {
            throw new InvalidOperationException("UseDispatcherEndpoints requires an IEndpointRouteBuilder host. Migrate to app.MapDispatcherEndpoints(...).");
        }

        routeBuilder.MapDispatcherEndpoints(builder);

        return app;
    }

    public static IDispatcherEndpointsBuilder Dispatch(this IEndpointsBuilder endpoints, Func<IDispatcherEndpointsBuilder, IDispatcherEndpointsBuilder> builder)
        => builder(new DispatcherEndpointsBuilder(endpoints));

    public static IApplicationBuilder UsePublicContracts<T>(this IApplicationBuilder app, string endpoint = "/_contracts")
        => app.UsePublicContracts(endpoint, typeof(T));

    public static IApplicationBuilder UsePublicContracts(this IApplicationBuilder app, bool attributeRequired, string endpoint = "/_contracts")
        => app.UsePublicContracts(endpoint, null, attributeRequired);

    public static IApplicationBuilder UsePublicContracts(this IApplicationBuilder app, string endpoint = "/_contracts", Type? attributeType = null, bool attributeRequired = true)
        => app.UseMiddleware<PublicContractsMiddleware>(string.IsNullOrWhiteSpace(endpoint) ? "/_contracts" : endpoint.StartsWith("/") ? endpoint : $"/{endpoint}", attributeType ?? typeof(PublicContractAttribute), attributeRequired);

    public static Task SendAsync<T>(this HttpContext context, T command)
        where T : class, ICommand
        => context.RequestServices.GetRequiredService<ICommandDispatcher>().SendAsync(command, context.RequestAborted);

    public static Task<TResult?> QueryAsync<TResult>(this HttpContext context, IQuery<TResult> query)
        => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync(query, context.RequestAborted);

    public static Task<TResult?> QueryAsync<TQuery, TResult>(this HttpContext context, TQuery query)
        where TQuery : class, IQuery<TResult>
        => context.RequestServices.GetRequiredService<IQueryDispatcher>().QueryAsync<TQuery, TResult>(query, context.RequestAborted);
}
