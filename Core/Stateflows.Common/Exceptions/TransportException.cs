using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class TransportException : StateflowsDefinitionException
    {
        public TransportException(string message) : base(message) { }
        public TransportException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
