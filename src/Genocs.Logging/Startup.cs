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
            Log.Logger = new LoggerConfiguration()
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
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
