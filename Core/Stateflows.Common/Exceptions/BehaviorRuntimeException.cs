using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    /// <summary>Reports a runtime error associated with a behavior class.</summary>
    public class BehaviorRuntimeException : StateflowsRuntimeException
    {
        /// <summary>Gets the behavior class associated with the error.</summary>
        public BehaviorClass BehaviorClass { get; }

        /// <summary>Creates a behavior-runtime exception.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorClass">The behavior class associated with the error.</param>
        public BehaviorRuntimeException(string message, BehaviorClass behaviorClass) : base(message)
        {
            BehaviorClass = behaviorClass;
        }

        /// <summary>Creates a behavior-runtime exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorClass">The behavior class associated with the error.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public BehaviorRuntimeException(string message, BehaviorClass behaviorClass, Exception innerException) : base(message, innerException)
        {
            BehaviorClass = behaviorClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
