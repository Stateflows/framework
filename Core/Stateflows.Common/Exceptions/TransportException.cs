using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports a transport or communication failure in a Stateflows integration.</summary>
    public class TransportException : StateflowsDefinitionException
    {
        /// <summary>Creates a transport exception.</summary>
        /// <param name="message">The error description.</param>
        public TransportException(string message) : base(message) { }
        /// <summary>Creates a transport exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public TransportException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
