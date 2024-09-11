using Stateflows.Common.Exceptions;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineRuntimeException : BehaviorRuntimeException
    {
        public StateMachineRuntimeException(string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
