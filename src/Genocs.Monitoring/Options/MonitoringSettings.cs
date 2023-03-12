namespace Genocs.Monitoring.Options
{
    /// <summary>
    /// The monitoring settings
    /// </summary>
    public class MonitoringSettings
    {
        /// <summary>
        /// The static position
        /// </summary>
        public static string Position = "Monitoring";

        /// <summary>
        /// Jaeger url
        /// </summary>
        public string Jaeger { get; set; } = "localhost";
    }
}


