namespace Genocs.Core.Builders.Options
{
    /// <summary>
    /// The application settings
    /// </summary>
    internal class AppSettings
    {
        /// <summary>
        /// The static position
        /// </summary>
        public static string Position = "App";

        /// <summary>
        /// Application name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the service
        /// </summary>
        public string Service { get; set; }
        public string Instance { get; set; }
        public string Version { get; set; }
        public bool DisplayBanner { get; set; }
        public bool DisplayVersion { get; set; }
    }
}
