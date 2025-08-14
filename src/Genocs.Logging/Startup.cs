using Serilog.Events;
using Serilog;

namespace Genocs.Logging;

/// <summary>
/// This helper function ensures that the logger is initialized.
/// </summary>
public static class StaticLogger
{
    /// <summary>
    /// This helper function ensures that the logger is initialized.
    /// Call this function before using the logger.
    /// Generally in the Program.cs file at the beginning of the application.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (Log.Logger is not Serilog.Core.Logger)
        {
#if DEBUG
            Log.Logger = CreateDebugLogger();
#else
            Log.Logger = CreateReleaseLogger();
#endif
        }
    }

#if DEBUG
    /// <summary>
    /// Creates a debug logger configuration with verbose logging for development.
    /// </summary>
    /// <returns>Configured Serilog logger for debug builds.</returns>
    private static Serilog.Core.Logger CreateDebugLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
            .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug)
            .MinimumLevel.Override("System", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.Http", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
#else
    /// <summary>
    /// Creates a release logger configuration with optimized logging for production.
    /// </summary>
    /// <returns>Configured Serilog logger for release builds.</returns>
    private static Serilog.Core.Logger CreateReleaseLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("MassTransit", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Http", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
#endif
}
