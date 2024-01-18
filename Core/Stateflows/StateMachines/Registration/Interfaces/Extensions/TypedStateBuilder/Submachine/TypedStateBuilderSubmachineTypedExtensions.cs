using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateBuilderSubmachineTypedExtensions
    {
        public static ISubmachineTypedStateBuilder AddSubmachine<TStateMachine>(this ITypedStateBuilder builder, StateActionInitializationBuilder initializationBuilder = null)
            where TStateMachine : StateMachine
            => builder.AddSubmachine(StateMachineInfo<TStateMachine>.Name, initializationBuilder);
    }
}
