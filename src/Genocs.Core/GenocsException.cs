namespace Genocs.Core
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception type for those are thrown by Abp system for Abp specific exceptions.
    /// </summary>
    [Serializable]
    public class GionatException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="GionatException"/> object.
        /// </summary>
        public GionatException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="GionatException"/> object.
        /// </summary>
        public GionatException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="GionatException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public GionatException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="GionatException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public GionatException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
