using System;

namespace Stateflows.Common.Exceptions
{
    public class StateflowsDefinitionException : StateflowsException
    {
        public StateflowsDefinitionException(string message) : base(message) { }
        public StateflowsDefinitionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
