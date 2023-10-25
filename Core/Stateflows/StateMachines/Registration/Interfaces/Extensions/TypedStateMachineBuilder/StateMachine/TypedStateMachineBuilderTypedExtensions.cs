using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedStateMachineBuilderTypedExtensions
    {
        #region AddFinalState
        [DebuggerHidden]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "<Pending>")]
        public static ITypedFinalizedStateMachineBuilder AddState<TFinalState>(this ITypedStateMachineBuilder builder, string stateName = FinalState.Name)
            where TFinalState : FinalState
            => builder.AddFinalState(stateName);
        #endregion

        #region AddState
        [DebuggerHidden]
        public static ITypedStateMachineBuilder AddState<TState>(this ITypedStateMachineBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddState<TState>(StateInfo<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static ITypedStateMachineBuilder AddState<TState>(this ITypedStateMachineBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
        {
            (builder as IInternal).Services.RegisterState<TState>();

            return builder.AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b as ITypedStateBuilder);
                }
            );
        }
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        public static ITypedStateMachineBuilder AddCompositeState<TCompositeState>(this ITypedStateMachineBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
            => builder.AddCompositeState<TCompositeState>(StateInfo<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static ITypedStateMachineBuilder AddCompositeState<TCompositeState>(this ITypedStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TCompositeState : CompositeState
        {
            (builder as IInternal).Services.RegisterState<TCompositeState>();

            return builder.AddCompositeState(
                stateName,
                b =>
                {
                    (b as ICompositeStateBuilder).AddStateEvents<TCompositeState, ICompositeStateBuilder>();
                    (b as ICompositeStateBuilder).AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateInitialBuilder);
                }
            );
        }
        #endregion
    }
}
