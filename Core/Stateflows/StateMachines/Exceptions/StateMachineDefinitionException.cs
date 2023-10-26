namespace Stateflows.StateMachines.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    internal class StateMachineDefinitionException : StateMachineException
    {
        public StateMachineDefinitionException(string message) : base(message) { }
    }
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
}
