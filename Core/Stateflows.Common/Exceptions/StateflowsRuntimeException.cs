using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class StateflowsRuntimeException : StateflowsException
    {
        public StateflowsRuntimeException(string message) : base(message) { }
        public StateflowsRuntimeException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
