using System.Runtime.Serialization;

namespace Genocs.Core.Exceptions;

/// <summary>
/// Base exception type for those are thrown by Genocs system for Genocs specific exceptions.
/// </summary>
[Serializable]
public class GenocsException : Exception
{
    /// <summary>
    /// Creates a new <see cref="GenocsException"/> object.
    /// </summary>
    public GenocsException()
    {

    }

    /// <summary>
    /// Creates a new <see cref="GenocsException"/> object.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public GenocsException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates a new <see cref="GenocsException"/> object.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception.</param>
    public GenocsException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a new <see cref="GenocsException"/> object.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public class InvalidConfigurationException(string message)
        : GenocsException(message);
}
