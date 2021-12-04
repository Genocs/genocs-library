namespace Genocs.Core
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception type for those are thrown by Abp system for Abp specific exceptions.
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
        /// <param name="message">Exception message</param>
        public GenocsException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="GenocsException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public GenocsException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
