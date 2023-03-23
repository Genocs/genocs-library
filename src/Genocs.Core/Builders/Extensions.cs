using Genocs.Common.Settings;
using Genocs.Common.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Builders;


/// <summary>
/// Builders Extensions
/// </summary>
public static class Extensions
{
    /// <summary>
    /// The Builder 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IGenocsBuilder AddGenocs(this IServiceCollection services, IConfiguration configuration = null)
    {
        var builder = GenocsBuilder.Create(services, configuration);
        var appOptions = builder.GetOptions<AppOptions>(AppOptions.Position);
        services.AddSingleton(appOptions);

        builder.Services.AddMemoryCache();
        services.AddSingleton<IServiceId, ServiceId>();
        if (!appOptions.DisplayBanner || string.IsNullOrWhiteSpace(appOptions.Name))
        {
            return builder;
        }
        var version = appOptions.DisplayVersion ? $" {appOptions.Version}" : string.Empty;
        Console.WriteLine(Figgle.FiggleFonts.Doom.Render($"{appOptions.Name}{version}"));

        return builder;
    }

    /// <summary>
    /// Run the application initializer
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseGenocs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
        Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();

        return app;
    }

    /// <summary>
    /// Get option helper method
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
        where TModel : new()
    {
        var model = new TModel();
        configuration.Bind(sectionName, model);
        return model;
    }

    /// <summary>
    /// Get option helper method
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="builder"></param>
    /// <param name="settingsSectionName"></param>
    /// <returns></returns>
    public static TModel GetOptions<TModel>(this IGenocsBuilder builder, string settingsSectionName)
        where TModel : new()
    {
        if (builder.Configuration != null)
        {
            return builder.Configuration.GetOptions<TModel>(settingsSectionName);
        }

        using var serviceProvider = builder.Services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return configuration.GetOptions<TModel>(settingsSectionName);
    }
}