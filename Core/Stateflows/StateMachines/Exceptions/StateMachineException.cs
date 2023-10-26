using Stateflows.Common.Exceptions;

namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineException : StateflowsException
    {
        public StateMachineException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
