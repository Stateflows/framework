using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderTyped2Extensions
    {
        #region AddInitialState
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IBaseState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
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
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IBaseState
            => builder.AddInitialCompositeState<TState>(State<TState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IBaseState
        {
            (builder as IInternal).Services.RegisterState2<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents2<TState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
                }
            );
        }
        #endregion
    }
}
