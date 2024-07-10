using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class TypedInitializedCompositeStateBuilderTypedExtensions
    {
        #region AddFinalState
        //[DebuggerHidden]
        //public static ITypedFinalizedCompositeStateBuilder AddState<TFinalState>(this ITypedInitializedCompositeStateBuilder builder, string stateName = FinalState.Name)
        //    where TFinalState : FinalState
        //    => builder.AddFinalState(stateName);
        #endregion

        #region AddState
        //[DebuggerHidden]
        //public static ITypedInitializedCompositeStateBuilder AddState<TState>(this ITypedInitializedCompositeStateBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
        //    where TState : State
        //    => builder.AddState<TState>(StateInfo<TState>.Name, stateBuildAction);

        //[DebuggerHidden]
        //public static ITypedInitializedCompositeStateBuilder AddState<TState>(this ITypedInitializedCompositeStateBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
        //    where TState : State
        //{
        //    (builder as IInternal).Services.RegisterState<TState>();

        //    return builder.AddState(
        //        stateName,
        //        b =>
        //        {
        //            b.AddStateEvents<TState, IStateBuilder>();

        //            stateBuildAction?.Invoke(b as ITypedStateBuilder);
        //        }
        //    );
        //}
        #endregion

        #region AddCompositeState
        //[DebuggerHidden]
        //public static ITypedInitializedCompositeStateBuilder AddCompositeState<TCompositeState>(this ITypedInitializedCompositeStateBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
        //    where TCompositeState : CompositeState
        //    => builder.AddCompositeState<TCompositeState>(StateInfo<TCompositeState>.Name, compositeStateBuildAction);

        //[DebuggerHidden]
        //public static ITypedInitializedCompositeStateBuilder AddCompositeState<TCompositeState>(this ITypedInitializedCompositeStateBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
        //    where TCompositeState : CompositeState
        //{
        //    (builder as IInternal).Services.RegisterState<TCompositeState>();

        //    return builder.AddCompositeState(
        //        stateName,
        //        b =>
        //        {
        //            (b as IInitializedCompositeStateBuilder).AddStateEvents<TCompositeState, IInitializedCompositeStateBuilder>();
        //            (b as IInitializedCompositeStateBuilder).AddCompositeStateEvents<TCompositeState, IInitializedCompositeStateBuilder>();

        //            compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
        //        }
        //    );
        //}
        #endregion
    }
}
