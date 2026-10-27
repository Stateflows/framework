using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports an error while serializing or deserializing Stateflows data.</summary>
    public class SerializationException : StateflowsRuntimeException
    {
        /// <summary>Creates a serialization exception.</summary>
        /// <param name="message">The error description.</param>
        public SerializationException(string message) : base(message) { }
        /// <summary>Creates a serialization exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public SerializationException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
