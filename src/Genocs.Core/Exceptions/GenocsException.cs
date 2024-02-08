using System.Runtime.Serialization;

namespace Genocs.Core.Exceptions;

/* Unmerged change from project 'Genocs.Core (netstandard2.0)'
Before:
    using System.Runtime.Serialization;
After:
    using System.Runtime.Serialization;
    using Genocs;
    using Genocs.Core;
    using Genocs.Core.Exceptions;
*/

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
    public GenocsException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
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
    public GenocsException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
