using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class SerializationException : StateflowsDefinitionException
    {
        public SerializationException(string message) : base(message) { }
        public SerializationException(string message, Exception innerException) : base(message, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
