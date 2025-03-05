using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineInitial<out TReturn>
    {
        #region AddInitialState
        /// <summary>
        /// Adds an initial state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">States</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddInitialState(string stateName, StateBuildAction stateBuildAction = null);

        /// <summary>
        /// Adds an initial state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">States</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="stateBuildAction">State build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialState<TState>(StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds an initial state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">States</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialState<TState>(string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            var result = AddInitialState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.VertexTypeAddedAsync<TState>(graph.Name, graph.Version, stateName));
            
            return result;
        }
        #endregion

        #region AddInitialCompositeState
        /// <summary>
        /// Adds an initial composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        /// <summary>
        /// Adds an initial composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialCompositeState<TCompositeState>(CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddInitialCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds an initial composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialCompositeState<TCompositeState>(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            var result = AddInitialCompositeState(
                compositeStateName,
                b =>
                {
                    b.AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.VertexTypeAddedAsync<TCompositeState>(graph.Name, graph.Version, compositeStateName));
            
            return result;
        }
        #endregion

        #region AddInitialOrthogonalState
        /// <summary>
        /// Adds an initial orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>
        /// Adds an initial orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialOrthogonalState<TOrthogonalState>(OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddInitialOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>
        /// Adds an initial orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn AddInitialOrthogonalState<TOrthogonalState>(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
        {
            var result = AddInitialOrthogonalState(
                orthogonalStateName,
                b =>
                {
                    b.AddOrthogonalStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();

                    orthogonalStateBuildAction?.Invoke(b);
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.VertexTypeAddedAsync<TOrthogonalState>(graph.Name, graph.Version, orthogonalStateName));
            
            return result;
        }
        #endregion
    }
}
