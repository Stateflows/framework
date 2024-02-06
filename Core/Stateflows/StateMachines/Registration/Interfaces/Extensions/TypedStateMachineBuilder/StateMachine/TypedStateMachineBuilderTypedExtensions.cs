using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedStateMachineBuilderTypedExtensions
    {
        #region AddInitialState
        [DebuggerHidden]
        public static ITypedInitializedStateMachineBuilder AddInitialState<TState>(this ITypedStateMachineBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : State
            => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedStateMachineBuilder AddInitialState<TState>(this ITypedStateMachineBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
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
        [DebuggerHidden]
        public static ITypedInitializedStateMachineBuilder AddInitialCompositeState<TState>(this ITypedStateMachineBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : State
            => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static ITypedInitializedStateMachineBuilder AddInitialCompositeState<TState>(this ITypedStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TState : State
        {
            (builder as IInternal).Services.RegisterState<TState>();

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
