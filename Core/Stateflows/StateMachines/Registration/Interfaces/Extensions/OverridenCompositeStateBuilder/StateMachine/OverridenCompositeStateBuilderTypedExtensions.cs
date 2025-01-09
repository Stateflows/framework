using System.Diagnostics;
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
        public static IOverridenCompositeStateBuilder AddState<TState>(this IOverridenCompositeStateBuilder builder, StateBuildAction stateBuildAction = null)
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
        public static IOverridenCompositeStateBuilder AddState<TState>(this IOverridenCompositeStateBuilder builder, string stateName, StateBuildAction stateBuildAction = null)
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
        public static IOverridenCompositeStateBuilder AddCompositeState<TCompositeState>(this IOverridenCompositeStateBuilder builder, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddCompositeState<TCompositeState>(this IOverridenCompositeStateBuilder builder, string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddCompositeState(
                compositeStateName,
                b =>
                {
                    b.AddStateEvents<TCompositeState, ICompositeStateBuilder>();
                    b.AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion

        #region AddOrthogonalState
        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOrthogonalState<TOrthogonalState>(this IOverridenCompositeStateBuilder builder, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => builder.AddOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>
        /// Adds initial orthogonal state to current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
        [DebuggerHidden]
        public static IOverridenCompositeStateBuilder AddOrthogonalState<TOrthogonalState>(this IOverridenCompositeStateBuilder builder, string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
        {
            (builder as IInternal).Services.AddServiceType<TOrthogonalState>();

            return builder.AddOrthogonalState(
                orthogonalStateName,
                b =>
                {
                    b.AddStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();
                    b.AddCompositeStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();

                    orthogonalStateBuildAction?.Invoke(b);
                }
            );
        }
        #endregion
    }
}
