using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedStateMachineInitialBuilderTypedExtensions
    {
        #region AddInitialState
        public static ITypedStateMachineBuilder AddInitialState<TState>(this ITypedStateMachineInitialBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        public static ITypedStateMachineBuilder AddInitialState<TState>(this ITypedStateMachineInitialBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            var self = builder as IStateMachineBuilderInternal;
            self.Services.RegisterState<TState>();

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
        public static ITypedStateMachineBuilder AddInitialCompositeState<TState>(this ITypedStateMachineInitialBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
            => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        public static ITypedStateMachineBuilder AddInitialCompositeState<TState>(this ITypedStateMachineInitialBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
        {
            var self = builder as IStateMachineBuilderInternal;
            self.Services.RegisterState<TState>();

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
