using System;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineOverrideException : StateMachineDefinitionException
    {
        public StateMachineClass StateMachineClass { get; }

        public StateMachineOverrideException(string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass.BehaviorClass)
        {
            StateMachineClass = stateMachineClass;
        }

        public StateMachineOverrideException(string message, StateMachineClass stateMachineClass, Exception innerException) : base(message, stateMachineClass.BehaviorClass, innerException)
        {
            StateMachineClass = stateMachineClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
