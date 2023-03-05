namespace Genocs.Monitoring.Options
{
    /// <summary>
    /// The monitoring settings
    /// </summary>
    public class MonitoringSettings
    {
        public static string Position = "Monitoring";

        public string Jaeger { get; set; } = "localhost";
    }
}


