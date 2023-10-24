using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class StateMachineInitialBuilderTypedExtensions
    {
        #region AddInitialState
        public static IStateMachineBuilder AddInitialState<TState>(this IStateMachineInitialBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        public static IStateMachineBuilder AddInitialState<TState>(this IStateMachineInitialBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            (builder as IInternal).Services.RegisterState<TState>();

            return builder.AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b as ITypedStateBuilder);
                }
            );
        }
        #endregion

        #region AddInitialCompositeState
        public static IStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineInitialBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
            => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        public static IStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineInitialBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
        {
            (builder as IInternal).Services.RegisterState<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    (b as ICompositeStateBuilder).AddStateEvents<TState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateInitialBuilder);
                }
            );
        }
        #endregion
    }
}
