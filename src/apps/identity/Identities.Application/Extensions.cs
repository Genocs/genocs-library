using System.Security.Claims;
using System.Text;
using Genocs.Auth;
using Genocs.Common.Configurations;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.HTTP;
using Genocs.Identities.Application.Commands;
using Genocs.Identities.Application.Decorators;
using Genocs.Identities.Application.Domain.Constants;
using Genocs.Identities.Application.Domain.Repositories;
using Genocs.Identities.Application.Exceptions;
using Genocs.Identities.Application.Logging;
using Genocs.Identities.Application.Mongo;
using Genocs.Identities.Application.Mongo.Documents;
using Genocs.Identities.Application.Mongo.Repositories;
using Genocs.Identities.Application.Services;
using Genocs.MessageBrokers;
using Genocs.MessageBrokers.CQRS;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.MongoDB;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Metrics.AppMetrics;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Persistence.Redis;
using Genocs.Tracing;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Genocs.Identities.Application;

public static class Extensions
{
    public static async Task<IGenocsBuilder> AddCoreAsync(this IGenocsBuilder builder)
    {
        builder.Services
            .AddScoped<LogContextMiddleware>()
            .AddSingleton<IJwtProvider, JwtProvider>()
            .AddSingleton<IPasswordService, PasswordService>()
            .AddSingleton<IPasswordHasher<IPasswordService>, PasswordHasher<IPasswordService>>()
            .AddSingleton<IRng, Rng>()
            .AddSingleton<ITokenStorage, TokenStorage>()
            .AddScoped<IMessageBroker, MessageBroker>()
            .AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
            .AddScoped<IUserRepository, UserRepository>();

        builder
            .AddErrorHandler<ExceptionToResponseMapper>()
            .AddExceptionToMessageMapper<ExceptionToMessageMapper>()
            .AddCommandHandlers()
            .AddEventHandlers()
            .AddQueryHandlers()
            .AddInMemoryCommandDispatcher()
            .AddInMemoryEventDispatcher()
            .AddInMemoryQueryDispatcher()
            .AddHttpClient();

        await builder.AddRabbitMQAsync();

        builder
            .AddOpenTelemetry()
            .AddMessageOutbox(o => o.AddMongo())
            .AddMongo()
            .AddRedis()
            .AddMetrics()
            .AddMongoRepository<RefreshTokenDocument, Guid>("refreshTokens")
            .AddMongoRepository<UserDocument, Guid>("users")
            .AddWebApiSwaggerDocs();

        builder.Services.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();

        builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(OutboxCommandHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(OutboxEventHandlerDecorator<>));

        builder.Services.AddAuthorizationBuilder()
                        .AddPolicy(Policies.AdminOnly, builder => builder.RequireClaim(ClaimTypes.Role, Roles.Admin).Build())
                        .AddPolicy(Policies.UserOnly, builder => builder.RequireClaim(ClaimTypes.Role, Roles.User).Build())
                        .AddPolicy(Policies.UserOrAdmin, builder => builder.RequireClaim(ClaimTypes.Role, Roles.User, Roles.Admin).Build());

        //builder.Services.AddAuthorizationBuilder()
        //                    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        //                    .RequireAuthenticatedUser()
        //                    .Build())
        //                        .AddPolicy(Policies.UserOnly, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, Roles.User)))
        //                        .AddPolicy(Policies.AdminOnly, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, Roles.Admin)))
        //                        .AddPolicy(Policies.UserOrAdmin, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, Roles.User)
        //                                || context.User.HasClaim(ClaimTypes.Role, Roles.Admin)));

        return builder;
    }

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

    public static IApplicationBuilder UseCore(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogContextMiddleware>()
            .UseErrorHandler()
            .UseSwaggerDocs()
            .UseGenocs()
            .UseAccessTokenValidator() // Implement the Authorization.
            .UseMongo()
            .UsePublicContracts<ContractAttribute>()
            .UseAuthentication()
            .UseMetrics()
            .UseRabbitMQ()
            .SubscribeCommand<CreateUser>()
            .SubscribeCommand<CreateAdmin>();

        return app;
    }

    public static async Task GetAppName(this HttpContext httpContext)
        => await httpContext.Response.WriteAsync(httpContext.RequestServices?.GetService<AppOptions>()?.Name ?? string.Empty);

    internal static CorrelationContext GetCorrelationContext(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.Request.Headers.TryGetValue("Correlation-Context", out var json) is true
            ? JsonConvert.DeserializeObject<CorrelationContext>(json.FirstOrDefault())
            : null;

    internal static string GetSpanContext(this IMessageProperties messageProperties, string header)
    {
        if (messageProperties is null)
        {
            return string.Empty;
        }

        if (messageProperties.Headers.TryGetValue(header, out object? span) && span is byte[] spanBytes)
        {
            return Encoding.UTF8.GetString(spanBytes);
        }

        return string.Empty;
    }
}