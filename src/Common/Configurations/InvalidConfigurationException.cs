namespace Genocs.Common.Configurations;

/// <summary>
/// Represents errors that occur during application configuration validation.
/// </summary>
/// <param name="message">The error message that explains the reason for the exception.</param>
public class InvalidConfigurationException(string message) : Exception(message);
