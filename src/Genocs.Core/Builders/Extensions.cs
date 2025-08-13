using System.Reflection;
using Genocs.Common.Configurations;
using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Genocs.Core.Builders;

/// <summary>
/// Builders Extensions.
/// </summary>
public static class Extensions
{
    public static IGenocsBuilder AddGenocs(this WebApplicationBuilder builder)
    {
        // Create the builder
        IGenocsBuilder gnxBuilder = GenocsBuilder.Create(builder);
        Setup(gnxBuilder);
        return gnxBuilder;
    }

    /// <summary>
    /// The Builder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The builder.</returns>
    public static IGenocsBuilder AddGenocs(this IServiceCollection services, IConfiguration? configuration = null)
    {
        // Create the builder
        IGenocsBuilder builder = GenocsBuilder.Create(services, configuration);
        Setup(builder);
        return builder;
    }

    /// <summary>
    /// Run the application initializer.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseGenocs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
        Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
        return app;
    }

    /// <summary>
    /// Get option helper method.
    /// </summary>
    /// <typeparam name="TModel">The option type parameter.</typeparam>
    /// <param name="configuration">The Configuration.</param>
    /// <param name="sectionName">The name of the section.</param>
    /// <returns>The option model or the default options.</returns>
    public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
        where TModel : new()
    {
        var model = new TModel();
        configuration.Bind(sectionName, model);
        return model;
    }

    /// <summary>
    /// Get option helper method.
    /// </summary>
    /// <typeparam name="TModel">The option type parameter.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="sectionName">The default section name.</param>
    /// <returns>The option.</returns>
    public static TModel GetOptions<TModel>(this IGenocsBuilder builder, string sectionName)
        where TModel : new()
    {
        if (builder.Configuration != null)
        {
            return builder.Configuration.GetOptions<TModel>(sectionName);
        }

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return configuration.GetOptions<TModel>(sectionName);
    }

    /// <summary>
    /// Map default endpoints to setup root endpoint and health checks.
    /// </summary>
    /// <param name="app">The web Application.</param>
    /// <returns>The WebApplication builder. You can use it for chain commands.</returns>
    public static IApplicationBuilder MapDefaultEndpoints(this IApplicationBuilder app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                // Get the Entry Assembly Name and Version
                // Check performance implications of calling this method
                string? assemblyVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                string? serviceVersion = context.RequestServices.GetService<AppOptions>()?.Name;
                string message = $"Service {serviceVersion ?? assemblyVersion} is running";

                await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>()?.Name ?? "Service");
            });

            // All health checks must pass for app to be considered ready to accept traffic after starting
            endpoints.MapHealthChecks("/healthz");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            endpoints.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        });

        return app;
    }

    /// <summary>
    /// Map default endpoints to setup health checks.
    /// </summary>
    /// <param name="app">The web Application.</param>
    /// <returns>The WebApplication builder. You can use it for chain commands.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapGet("/", async context =>
        {
            // Get the Entry Assembly Name and Version
            // Check performance implications of calling this method
            string? assemblyVersion = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            string? serviceVersion = context.RequestServices.GetService<AppOptions>()?.Name;
            string message = $"Service {serviceVersion ?? assemblyVersion} is running";

            await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>()?.Name ?? message);
        }).AllowAnonymous();

        // All health checks must pass for app to be considered ready to accept traffic after starting
        app.MapHealthChecks("/healthz").AllowAnonymous();

        // Only health checks tagged with the "live" tag must pass for app to be considered alive
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        }).AllowAnonymous();

        return app;
    }

    private static void Setup(IGenocsBuilder builder)
    {
        // Get the application options
        AppOptions settings = builder.GetOptions<AppOptions>(AppOptions.Position);
        builder.Services.AddSingleton(settings);

        if (!settings.DisplayBanner || string.IsNullOrWhiteSpace(settings.Name))
        {
            return;
        }

        string version = settings.DisplayVersion ? $" {settings.Version}" : string.Empty;
        Console.WriteLine(Figgle.Fonts.FiggleFonts.Doom.Render(settings.Name + version));
        ConsoleColor current = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Runtime Version: {0}", Environment.Version.ToString());
        Console.ForegroundColor = current;

        // Add the health checks
        // Add health checks to the application
        // Since the health checks is item potent, we can add it multiple times
        // Add a default liveness check to ensure app is responsive
        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

            /*
             * This is an example of how to add a MongoDB health check.
             * Please note that you need to install the NuGet package AspNetCore.HealthChecks.MongoDb
             * 
            .AddMongoDb(
                builder.Configuration.GetSection("DBSettings:HealthConnectionString").Value!,
                builder.Configuration.GetSection("DBSettings:Database").Value!,
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(10),
                tags: ["live"]);

            */

        // Set the memory cache as default.
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<IServiceId, ServiceId>();
    }
}