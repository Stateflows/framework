using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Base class for Stateflows runtime failures.</summary>
    public class StateflowsRuntimeException : StateflowsException
    {
        /// <summary>Creates a runtime exception.</summary>
        /// <param name="message">The error description.</param>
        public StateflowsRuntimeException(string message) : base(message) { }
        /// <summary>Creates a runtime exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public StateflowsRuntimeException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
