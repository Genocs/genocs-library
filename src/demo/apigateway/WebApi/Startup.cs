using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Messaging.RabbitMQ;
using Genocs.Security;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.Persistence.MongoDB.Extensions;
using Yarp.ReverseProxy.Forwarder;
using Genocs.APIGateway.WebApi.Configurations;
using Genocs.APIGateway.WebApi.Framework;

namespace Genocs.APIGateway.WebApi;

internal class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        // Find a more elegant way to do this
        Task.Run(async () => await ConfigureServicesAsync(services)).Wait();
    }

    private async Task ConfigureServicesAsync(IServiceCollection services)
    {
        services.AddScoped<LogContextMiddleware>();
        services.AddScoped<UserMiddleware>();
        services.AddScoped<MessagingMiddleware>();
        services.AddSingleton<CorrelationIdFactory>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<ICorrelationContextBuilder, CorrelationContextBuilder>();
        services.AddSingleton<RouteMatcher>();
        services.Configure<MessagingOptions>(Configuration.GetSection(MessagingOptions.Position));

        services.AddSingleton<IForwarderHttpClientFactory, CustomForwarderHttpClientFactory>();

        var builder = services
                            .AddGenocs(Configuration)
                            .AddTelemetry()
                            .AddMongoWithRegistration()
                            .AddJwt();

        await builder.AddRabbitMQAsync();

        builder.AddSecurity()
            .AddWebApi();

        // Complete the configuration of Genocs and build the service provider
        // .Build();

        services.AddReverseProxy()
                .LoadFromConfig(Configuration.GetSection("ReverseProxy"));

        // Alternatively, if you want to load the configuration from a database or another source,
        // you can implement a custom IProxyConfigProvider and register it here. For example:
        // .LoadFromDatabase(Configuration);

        // services.AddAuthorization(options =>
        // {
        //     options.AddPolicy("authenticatedUser", policy =>
        //         policy.RequireAuthenticatedUser());
        // });

        services.AddCors(cors =>
        {
            cors.AddPolicy("cors", x =>
            {
                x.AllowAnyOrigin()
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS", "HEAD")
                    .WithHeaders("Content-Type", "Authorization", "x-correlation-id");
            });
        });

        services.AddHealthChecks();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<LogContextMiddleware>();
        app.UseCors("cors");
        app.UseGenocs();
        app.UsePrometheus();
        app.UseAccessTokenValidator();
        app.UseAuthentication();
        app.UseRabbitMQ();
        app.UseMiddleware<UserMiddleware>();
        app.UseMiddleware<MessagingMiddleware>();
        app.UseRouting();
        app.UseAuthorization();

        app.MapDefaultEndpoints();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapPrometheus();
            endpoints.MapReverseProxy();
        });
    }
}
