﻿using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class OverridenOverridenCompositeStateBuilderTypedExtensions
    {
        #region AddFinalState
        /// <summary>
        /// Adds final state to current composite state.
        /// </summary>
        /// <typeparam name="TFinalState"><see cref="FinalState"/> class</typeparam>
        /// <param name="finalStateName">Final state name</param>
        [DebuggerHidden]
        public static IFinalizedOverridenCompositeStateBuilder AddState<TFinalState>(this IOverridenCompositeStateBuilder builder, string finalStateName = FinalState.Name)
            where TFinalState : class, IFinalState
            => builder.AddFinalState(finalStateName);
        #endregion

        #region AddState
        /// <summary>
        /// Adds state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddState<TState>(this IOverridenCompositeStateBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddState<TState>(this IOverridenCompositeStateBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
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
        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddCompositeState<TCompositeState>(this IOverridenCompositeStateBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddCompositeState<TCompositeState>(this IOverridenCompositeStateBuilder builder, string compositeStateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddCompositeState(
                compositeStateName,
                b =>
                {
                    (b as IOverridenCompositeStateBuilder).AddStateEvents<TCompositeState, IOverridenCompositeStateBuilder>();
                    (b as IOverridenCompositeStateBuilder).AddCompositeStateEvents<TCompositeState, IOverridenCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
