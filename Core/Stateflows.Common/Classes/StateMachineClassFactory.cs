namespace Stateflows
{
    public struct StateMachineClassFactory
    {
        public readonly StateMachineClass ToClass(string name) => new StateMachineClass(name);

        public static implicit operator string(StateMachineClassFactory _) => "StateMachine";
    }
}
