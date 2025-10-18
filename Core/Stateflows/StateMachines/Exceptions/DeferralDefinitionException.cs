namespace Stateflows.StateMachines.Exceptions
{
    internal class DeferralDefinitionException : StateMachineDefinitionException
    {
        public DeferralDefinitionException(string eventName, string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass)
        {
            EventName = eventName;
        }

        public string EventName { get; set; }
    }
}
