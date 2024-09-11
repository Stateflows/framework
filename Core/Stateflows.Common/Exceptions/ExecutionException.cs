using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ExecutionException : StateflowsDefinitionException
    {
        public ExecutionException(Exception innerException) : base(string.Empty, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
