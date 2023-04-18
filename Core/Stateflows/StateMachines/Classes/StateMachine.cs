namespace Stateflows.StateMachines
{
    public abstract class StateMachine
    {
        public abstract void Build(IStateMachineInitialBuilder builder);
    }

    public sealed class StateMachineInfo<TStateMachine>
        where TStateMachine : StateMachine
    {
        public static string Name { get => typeof(TStateMachine).Name; }
    }
}
