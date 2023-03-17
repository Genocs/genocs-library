namespace Genocs.Core.Options
{
    /// <summary>
    /// The appplication Settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Application name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Service name
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// The instance of the service
        /// </summary>
        public string Instance { get; set; }
        public string Version { get; set; }
        public bool DisplayBanner { get; set; } = true;
        public bool DisplayVersion { get; set; } = true;
    }
}