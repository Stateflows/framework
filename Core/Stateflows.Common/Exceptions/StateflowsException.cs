using System;

namespace Stateflows.Common.Exceptions
{
    public class StateflowsException : Exception
    {
        public StateflowsException(string message) : base(message) { }
        public StateflowsException(string message, Exception innerException) : base(message, innerException) { }
    }
}
