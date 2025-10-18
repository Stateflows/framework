namespace Stateflows.StateMachines.Exceptions
{
    internal class StateDefinitionException : StateMachineDefinitionException
    {
        public StateDefinitionException(string stateName, string message, StateMachineClass stateMachineClass) : base(message, stateMachineClass)
        {
            StateName = stateName;
        }

        public string StateName { get; set; }
    }
}
