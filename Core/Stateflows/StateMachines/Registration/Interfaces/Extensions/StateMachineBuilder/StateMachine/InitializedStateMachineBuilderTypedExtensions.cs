using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class InitializedStateMachineBuilderTypedExtensions
    {
        #region AddFinalState
        [DebuggerHidden]
        public static IFinalizedStateMachineBuilder AddState<TFinalState>(this IInitializedStateMachineBuilder builder, string stateName = FinalState.Name)
            where TFinalState : class, IFinalState
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
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
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
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddCompositeState(
                stateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents<TCompositeState, IInitializedCompositeStateBuilder>();
                    (b as IInitializedCompositeStateBuilder).AddCompositeStateEvents<TCompositeState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
