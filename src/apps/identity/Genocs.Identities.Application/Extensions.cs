using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Core.Settings;
using Genocs.HTTP;
using Genocs.Identities.Application.Commands;
using Genocs.Identities.Application.Decorators;
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
using Genocs.Persistence.MongoDb.Legacy;
using Genocs.Persistence.Redis;
using Genocs.Tracing.Jaeger;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;

namespace Genocs.Identities.Application;

public static class Extensions
{
    public static IGenocsBuilder AddCore(this IGenocsBuilder builder)
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
            .AddJwt()
            .AddHttpClient()
            .AddRabbitMq()
            .AddMessageOutbox(o => o.AddMongo())
            .AddMongo()
            .AddRedis()
            .AddJaeger()
            .AddMetrics()
            .AddMongoRepository<RefreshTokenDocument, Guid>("refreshTokens")
            .AddMongoRepository<UserDocument, Guid>("users")
            .AddWebApiSwaggerDocs();

        builder.Services.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();

        builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(ICommandHandler<>), typeof(OutboxCommandHandlerDecorator<>));
        builder.Services.TryDecorate(typeof(IEventHandler<>), typeof(OutboxEventHandlerDecorator<>));

        return builder;
    }

    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

    public static IApplicationBuilder UseCore(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogContextMiddleware>()
            .UseErrorHandler()
            .UseJaeger()
            .UseSwaggerDocs()
            .UseGenocs()
            .UseAccessTokenValidator()
            .UseMongo()
            .UsePublicContracts<ContractAttribute>()
            .UseAuthentication()
            .UseMetrics()
            .UseRabbitMq()
            .SubscribeCommand<CreateUser>();

        return app;
    }

    public static async Task GetAppName(this HttpContext httpContext)
        => await httpContext.Response.WriteAsync(httpContext.RequestServices?.GetService<AppOptions>()?.Name ?? "ciao");

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

        if (messageProperties.Headers.TryGetValue(header, out var span) && span is byte[] spanBytes)
        {
            return Encoding.UTF8.GetString(spanBytes);
        }

        return string.Empty;
    }
}