using Genocs.Core.Builders;
using Genocs.Tracing.Jaeger.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Tracing.Jaeger;

/// <summary>
/// The Open Tracing.
/// </summary>
public static class Extensions
{
    private static int _initialized;

    /// <summary>
    /// Add Jaeger Tracer.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IGenocsBuilder AddJaeger(this IGenocsBuilder builder, string sectionName = JaegerOptions.Position)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return builder;
        }

        var options = builder.GetOptions<JaegerOptions>(sectionName);

        builder.Services.AddSingleton(options);

        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }

    public static IApplicationBuilder UseJaeger(this IApplicationBuilder app)
    {
        // Could be extended with some additional middleware
        using var scope = app.ApplicationServices.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<JaegerOptions>();

        return app;
    }
}