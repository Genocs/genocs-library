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
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
