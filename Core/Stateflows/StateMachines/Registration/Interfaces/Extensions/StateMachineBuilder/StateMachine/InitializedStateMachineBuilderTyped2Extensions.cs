using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class InitializedStateMachineBuilderTyped2Extensions
    {
        #region AddFinalState
        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddState<TFinalState>(this IInitializedStateMachineBuilder builder, string stateName = FinalState.Name)
            where TFinalState : IFinalState
            => builder.AddFinalState(stateName);
        #endregion

        #region AddState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddState<TState>(this IInitializedStateMachineBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddState<TState>(this IInitializedStateMachineBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            (builder as IInternal).Services.RegisterState2<TState>();

            return builder.AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents2<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b as ITypedStateBuilder);
                }
            );
        }
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddCompositeState<TCompositeState>(this IInitializedStateMachineBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IInitializedStateMachineBuilder AddCompositeState<TCompositeState>(this IInitializedStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            (builder as IInternal).Services.RegisterState2<TCompositeState>();

            return builder.AddCompositeState(
                stateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents2<TCompositeState, IInitializedCompositeStateBuilder>();
                    (b as IInitializedCompositeStateBuilder).AddCompositeStateEvents2<TCompositeState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
                }
            );
        }
        #endregion
    }
}
