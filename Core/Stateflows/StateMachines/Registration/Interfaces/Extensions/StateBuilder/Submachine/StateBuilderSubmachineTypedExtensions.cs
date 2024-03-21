using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class StateBuilderSubmachineTypedExtensions
    {
        public static IBehaviorStateBuilder AddSubmachine<TStateMachine>(this IStateBuilder builder, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, buildAction, initializationBuilder);
    }
}
