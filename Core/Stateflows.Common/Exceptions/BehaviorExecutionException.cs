using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Wraps an exception raised while executing a behavior.</summary>
    public class BehaviorExecutionException : StateflowsRuntimeException
    {
        /// <summary>Creates an exception for a failed behavior execution.</summary>
        /// <param name="innerException">The exception raised during execution.</param>
        public BehaviorExecutionException(Exception innerException) : base(string.Empty, innerException) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
