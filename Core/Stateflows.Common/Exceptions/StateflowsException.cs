using System;

namespace Stateflows.Common.Exceptions
{
    /// <summary>Base exception for errors raised by Stateflows.</summary>
    public class StateflowsException : Exception
    {
        /// <summary>Creates a Stateflows exception.</summary>
        /// <param name="message">The error description.</param>
        public StateflowsException(string message) : base(message) { }
        /// <summary>Creates a Stateflows exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public StateflowsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
