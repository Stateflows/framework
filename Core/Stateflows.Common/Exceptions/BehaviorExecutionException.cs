using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class BehaviorExecutionException : StateflowsRuntimeException
    {
        public BehaviorExecutionException(Exception innerException) : base(string.Empty, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
