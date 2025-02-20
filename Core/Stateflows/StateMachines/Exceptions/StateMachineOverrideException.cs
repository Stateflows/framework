using System;

namespace Stateflows.StateMachines.Exceptions
{
    internal class StateMachineOverrideException : StateMachineDefinitionException
    {
        public new StateMachineClass StateMachineClass { get; }

        public StateMachineOverrideException(string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass.BehaviorClass)
        {
            StateMachineClass = stateMachineClass;
        }

        public StateMachineOverrideException(string message, StateMachineClass stateMachineClass, Exception innerException) : base(message, stateMachineClass.BehaviorClass, innerException)
        {
            StateMachineClass = stateMachineClass;
        }
    }
}
