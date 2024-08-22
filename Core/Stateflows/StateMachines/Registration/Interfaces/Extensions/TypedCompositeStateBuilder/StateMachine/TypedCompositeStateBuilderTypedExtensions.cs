using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedCompositeStateBuilderTypedExtensions
    {
        #region AddInitialState
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

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
        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IState
            => builder.AddInitialCompositeState<TState>(State<TState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : class, IState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddInitialCompositeState(
                stateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents<TState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
                }
            );
        }
        #endregion
    }
}
