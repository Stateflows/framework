using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderSubmachineTypedExtensions
    {
        public static IBehaviorTypedStateBuilder AddSubmachine<TStateMachine>(this ITypedStateBuilder builder, EmbeddedBehaviorBuildAction buildAction, StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : class, IStateMachine
            => builder.AddSubmachine(StateMachine<TStateMachine>.Name, buildAction, initializationBuilder);
    }
}
