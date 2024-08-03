using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Typed
{
    public static class CompositeStateBuilderTypedExtensions
    {
        #region AddInitialState
        /// <summary>
        /// Adds initial state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddInitialState<TState>(this ICompositeStateBuilder builder, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IBaseState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds initial state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddInitialState<TState>(this ICompositeStateBuilder builder, string stateName, StateTransitionsBuildAction stateBuildAction = null)
            where TState : class, IBaseState
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
        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddInitialCompositeState<TCompositeState>(this ICompositeStateBuilder builder, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, IBaseCompositeState
            => builder.AddInitialCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IInitializedCompositeStateBuilder AddInitialCompositeState<TCompositeState>(this ICompositeStateBuilder builder, string compositeStateName, CompositeStateTransitionsBuildAction compositeStateBuildAction)
            where TCompositeState : class, IBaseCompositeState
        {
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddInitialCompositeState(
                compositeStateName,
                b =>
                {
                    (b as IInitializedCompositeStateBuilder).AddStateEvents<TCompositeState, IInitializedCompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b as ITypedCompositeStateBuilder);
                }
            );
        }
        #endregion
    }
}
