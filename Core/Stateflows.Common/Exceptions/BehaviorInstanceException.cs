using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports a runtime error associated with a specific behavior instance.</summary>
    public class BehaviorInstanceException : BehaviorRuntimeException
    {
        /// <summary>Gets the identifier of the behavior instance that failed.</summary>
        public BehaviorId BehaviorId { get; }

        /// <summary>Creates a behavior-instance exception.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorId">The behavior instance associated with the error.</param>
        public BehaviorInstanceException(string message, BehaviorId behaviorId) : base(message, behaviorId.BehaviorClass)
        {
            BehaviorId = behaviorId;
        }

        /// <summary>Creates a behavior-instance exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorId">The behavior instance associated with the error.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public BehaviorInstanceException(string message, BehaviorId behaviorId, Exception innerException) : base(message, behaviorId.BehaviorClass, innerException)
        {
            BehaviorId = behaviorId;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
