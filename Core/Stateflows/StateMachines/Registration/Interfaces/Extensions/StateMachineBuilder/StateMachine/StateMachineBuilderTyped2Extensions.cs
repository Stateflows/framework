using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class StateMachineBuilderTyped2Extensions
    {
        #region AddInitialState
        public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IBaseState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        public static IInitializedStateMachineBuilder AddInitialState<TState>(this IStateMachineBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IBaseState
        {
            (builder as IInternal).Services.RegisterState2<TState>();

            return builder.AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents2<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b as ITypedStateBuilder);
                }
            );
        }
        #endregion

        #region AddInitialCompositeState
        public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IBaseCompositeState
            => builder.AddInitialCompositeState<TState>(State<TState>.Name, compositeStateBuildAction);

        public static IInitializedStateMachineBuilder AddInitialCompositeState<TState>(this IStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IBaseCompositeState
        {
            (builder as IInternal).Services.RegisterState2<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents2<TState, IInitializedCompositeStateBuilder>();
                    (b as IInitializedCompositeStateBuilder).AddCompositeStateEvents2<TState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
                }
            );
        }
        #endregion
    }
}
