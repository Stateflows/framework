using System.Diagnostics;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines
{
    public static class RegionBuilderTypedExtensions
    {
        #region AddInitialState
        /// <summary>
        /// Adds initial state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public static IInitializedRegionBuilder AddInitialState<TState>(this IRegionBuilder builder, StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => builder.AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds initial state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public static IInitializedRegionBuilder AddInitialState<TState>(this IRegionBuilder builder, string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            (builder as IInternal).Services.AddServiceType<TState>();

            return builder.AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
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
        public static IInitializedRegionBuilder AddInitialCompositeState<TCompositeState>(this IRegionBuilder builder, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => builder.AddInitialCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IInitializedRegionBuilder AddInitialCompositeState<TCompositeState>(this IRegionBuilder builder, string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            (builder as IInternal).Services.AddServiceType<TCompositeState>();

            return builder.AddInitialCompositeState(
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

        #region AddInitialOrthogonalState
        /// <summary>
        /// Adds initial orthogonal state to current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
        [DebuggerHidden]
        public static IInitializedRegionBuilder AddInitialOrthogonalState<TOrthogonalState>(this IRegionBuilder builder, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => builder.AddInitialOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>
        /// Adds initial orthogonal state to current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public static IInitializedRegionBuilder AddInitialOrthogonalState<TOrthogonalState>(this IRegionBuilder builder, string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
        {
            (builder as IInternal).Services.AddServiceType<TOrthogonalState>();

            return builder.AddInitialOrthogonalState(
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
