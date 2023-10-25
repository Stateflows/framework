using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderSubmachineTypedExtensions
    {
        public static ISubmachineStateBuilder AddSubmachine<TStateMachine>(this IStateBuilder builder, StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, initializationBuilder);
    }
}
