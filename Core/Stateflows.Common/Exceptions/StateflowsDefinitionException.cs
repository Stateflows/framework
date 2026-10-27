using System;

namespace Stateflows.Common.Exceptions
{
    /// <summary>Reports an error in a Stateflows definition or configuration.</summary>
    public class StateflowsDefinitionException : StateflowsException
    {
        /// <summary>Creates a definition exception.</summary>
        /// <param name="message">The error description.</param>
        public StateflowsDefinitionException(string message) : base(message) { }
        /// <summary>Creates a definition exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public StateflowsDefinitionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
