using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class TypedCompositeStateInitialBuilderTypedExtensions
    {
        #region AddInitialState
        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateInitialBuilder builder, StateTransitionsBuilderAction stateBuildAction = null)
            where TState : State
            => builder.AddInitialState<TState>(StateInfo<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddInitialState<TState>(this ITypedCompositeStateInitialBuilder builder, string stateName, StateTransitionsBuilderAction stateBuildAction = null)
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
        public static ITypedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateInitialBuilder builder, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
            where TState : State
            => builder.AddInitialCompositeState<TState>(StateInfo<TState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static ITypedCompositeStateBuilder AddInitialCompositeState<TState>(this ITypedCompositeStateInitialBuilder builder, string stateName, CompositeStateTransitionsBuilderAction compositeStateBuildAction)
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
