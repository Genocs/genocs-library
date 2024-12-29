using Genocs.APIGateway.Configurations;
using Genocs.APIGateway.Framework;
using Genocs.APIGateway.Providers;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Metrics.Prometheus;
using Genocs.Security;
using Genocs.Tracing;
using Genocs.WebApi;
using Genocs.Persistence.MongoDb.Extensions;
using Yarp.ReverseProxy.Forwarder;

namespace Genocs.APIGateway;

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

        IGenocsBuilder builder = services
                                        .AddGenocs(Configuration)
                                        .AddOpenTelemetry()
                                        .AddMongo()
                                        .AddMongoFast()
                                        .AddJwt()
                                        .AddPrometheus();

        await builder.AddRabbitMQAsync();

        builder.AddSecurity()
            .AddWebApi()
            .Build();

        services.AddReverseProxy()
                .LoadFromDatabase(Configuration);

        //services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("authenticatedUser", policy =>
        //        policy.RequireAuthenticatedUser());
        //});

        services.AddCors(cors =>
        {
            cors.AddPolicy("cors", x =>
            {
                x.WithOrigins("*")
                    .WithMethods("POST", "PUT", "DELETE")
                    .WithHeaders("Content-Type", "Authorization");
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
            endpoints.MapReverseProxy();
        });
    }
}
