using System;

namespace Stateflows.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class BehaviorDefinitionException : StateflowsDefinitionException
    {
        public BehaviorClass BehaviorClass { get; }

        public BehaviorDefinitionException(string message, BehaviorClass behaviorClass) : base(message)
        {
            BehaviorClass = behaviorClass;
        }

        public BehaviorDefinitionException(string message, BehaviorClass behaviorClass, Exception innerException) : base(message, innerException)
        {
            BehaviorClass = behaviorClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
