﻿using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class OverridenStateMachineBuilderTypedExtensions
    {
        #region AddFinalState
        [DebuggerHidden]
        public static IFinalizedOverridenStateMachineBuilder AddState<TFinalState>(this IOverridenStateMachineBuilder builder, string stateName = FinalState.Name)
            where TFinalState : class, IFinalState
            => builder.AddFinalState(stateName);
        #endregion

        #region AddState
        [DebuggerHidden]
        public static IOverridenStateMachineBuilder AddState<TState>(this IOverridenStateMachineBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddState<TState>(State<TState>.Name, stateBuildAction);

        [DebuggerHidden]
        public static IOverridenStateMachineBuilder AddState<TState>(this IOverridenStateMachineBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
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
        public static IOverridenStateMachineBuilder AddCompositeState<TCompositeState>(this IOverridenStateMachineBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        [DebuggerHidden]
        public static IOverridenStateMachineBuilder AddCompositeState<TCompositeState>(this IOverridenStateMachineBuilder builder, string stateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
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
