using Genocs.Common.Configurations;
using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Builders;

/// <summary>
/// Builders Extensions.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// The Builder.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns>The builder.</returns>
    public static IGenocsBuilder AddGenocs(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var builder = GenocsBuilder.Create(services, configuration);
        var settings = builder.GetOptions<AppOptions>(AppOptions.Position);
        services.AddSingleton(settings);

        builder.Services.AddMemoryCache();
        services.AddSingleton<IServiceId, ServiceId>();
        if (!settings.DisplayBanner || string.IsNullOrWhiteSpace(settings.Name))
        {
            return builder;
        }

        string version = settings.DisplayVersion ? $" {settings.Version}" : string.Empty;
        Console.WriteLine(Figgle.FiggleFonts.Doom.Render(settings.Name + version));
        ConsoleColor current = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Runtime Version: {0}", Environment.Version.ToString());
        Console.ForegroundColor = current;

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
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
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
}