using System;

namespace Stateflows.Common.Exceptions
{
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
}
