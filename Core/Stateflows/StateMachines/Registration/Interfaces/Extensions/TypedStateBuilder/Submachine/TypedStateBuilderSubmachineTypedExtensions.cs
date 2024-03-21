using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderSubmachineTypedExtensions
    {
        public static IBehaviorTypedStateBuilder AddSubmachine<TStateMachine>(this ITypedStateBuilder builder, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, buildAction, initializationBuilder);
    }
}
