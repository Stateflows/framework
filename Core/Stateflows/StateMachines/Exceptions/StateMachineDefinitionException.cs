using System;
using Stateflows.Common.Exceptions;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineDefinitionException : BehaviorDefinitionException
    {
        public StateMachineClass StateMachineClass { get; }

        public StateMachineDefinitionException(string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass.BehaviorClass)
        {
            StateMachineClass = stateMachineClass;
        }

        public StateMachineDefinitionException(string message, StateMachineClass stateMachineClass, Exception innerException) : base(message, stateMachineClass.BehaviorClass, innerException)
        {
            StateMachineClass = stateMachineClass;
        }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
