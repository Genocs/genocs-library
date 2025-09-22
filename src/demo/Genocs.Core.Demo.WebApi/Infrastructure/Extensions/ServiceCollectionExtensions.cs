﻿using Genocs.Common.Persistence.Initialization;
using Genocs.Core.Demo.WebApi.Configurations;
using MassTransit;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        RabbitMQOptions rabbitMQSettings = new RabbitMQOptions();
        configuration.GetSection(RabbitMQOptions.Position).Bind(rabbitMQSettings);

        // This is another way to get the RabbitMQOptions
        // RabbitMQOptions? rabbitMQSettingsV2 = configuration.GetSection(RabbitMQOptions.Position).Get<RabbitMQOptions>();

        services.AddSingleton(rabbitMQSettings);

        services.AddMassTransit(x =>
        {
            //x.AddConsumersFromNamespaceContaining<MerchantStatusChangedEvent>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);

                // cfg.UseHealthCheck(context);
                cfg.Host(
                            rabbitMQSettings.HostName,
                            rabbitMQSettings.VirtualHost,
                            h =>
                            {
                                h.Username(rabbitMQSettings.UserName);
                                h.Password(rabbitMQSettings.Password);
                            });
            });
        });

        return services;
    }

    public static IServiceCollection AddFirebaseAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseAuthorizationOptions>(configuration.GetSection(FirebaseAuthorizationOptions.Position));
        return services;
    }

    public static IApplicationBuilder UseFirebaseAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirebaseAuthenticationMiddleware>();
    }

    public static IApplicationBuilder UseFirebaseAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FirebaseAuthorizationMiddleware>();
    }

    /// <summary>
    /// Initialize the databases.
    /// </summary>
    /// <param name="services">The Service Provider.</param>
    /// <param name="cancellationToken">The cancellation Token.</param>
    /// <returns></returns>
    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }
}
