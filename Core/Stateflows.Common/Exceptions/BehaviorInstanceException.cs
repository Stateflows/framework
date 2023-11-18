using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class BehaviorInstanceException : BehaviorException
    {
        public BehaviorId BehaviorId { get; }

        public BehaviorInstanceException(string message, BehaviorId behaviorId) : base(message, behaviorId.BehaviorClass)
        {
            BehaviorId = behaviorId;
        }

        public BehaviorInstanceException(string message, BehaviorId behaviorId, Exception innerException) : base(message, behaviorId.BehaviorClass, innerException)
        {
            BehaviorId = behaviorId;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
