using System;

namespace Stateflows.Common.Exceptions
{
    /// <summary>Reports an error in the definition of a behavior class.</summary>
    public class BehaviorDefinitionException : StateflowsDefinitionException
    {
        /// <summary>Gets the behavior class whose definition failed.</summary>
        public BehaviorClass BehaviorClass { get; }

        /// <summary>Creates a behavior-definition exception.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorClass">The behavior class with the invalid definition.</param>
        public BehaviorDefinitionException(string message, BehaviorClass behaviorClass) : base(message)
        {
            BehaviorClass = behaviorClass;
        }

        /// <summary>Creates a behavior-definition exception that wraps another error.</summary>
        /// <param name="message">The error description.</param>
        /// <param name="behaviorClass">The behavior class with the invalid definition.</param>
        /// <param name="innerException">The exception that caused this failure.</param>
        public BehaviorDefinitionException(string message, BehaviorClass behaviorClass, Exception innerException) : base(message, innerException)
        {
            BehaviorClass = behaviorClass;
        }
    }
}
