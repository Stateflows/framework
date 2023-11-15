namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineDefinitionException : StateMachineException
    {
        public StateMachineDefinitionException(string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
