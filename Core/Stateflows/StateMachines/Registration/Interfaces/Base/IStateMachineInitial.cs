using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds and selects the initial state or pseudostate for a machine or nested region.</summary>
    /// <typeparam name="TReturn">The initialized definition stage returned after the initial vertex is selected.
    /// </typeparam>
    /// <remarks>This selection is part of registration, not an immediate runtime entry into the state.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IStateMachineInitial<out TReturn>
    {
        #region AddInitialState
        /// <summary>Adds an initial state to the current machine or region.</summary>
        /// <param name="stateName">Registered name of the regular state within the machine or region.</param>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage after selecting its initial state.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddInitialState(string stateName, StateBuildAction stateBuildAction = null);
        
        /// <summary>Adds an initial state to the current machine or region.</summary>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage after selecting its initial state.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddInitialState(StateBuildAction stateBuildAction = null)
            => AddInitialState(null, stateBuildAction);

        /// <summary>Adds an initial state to the current machine or region.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />. The registered name is resolved
        /// from the type's naming metadata and settings.</typeparam>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn AddInitialState<TState>(StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddInitialState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>Adds an initial state to the current machine or region.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />.</typeparam>
        /// <param name="stateName">Registered name of the regular state within the machine or region.</param>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
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
        /// <summary>Adds an initial composite state to the current machine or region.</summary>
        /// <param name="compositeStateName">Registered name of the composite state within the machine or region.</param>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized stage, not the nested composite builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        /// <summary>Adds an initial composite state to the current machine or region.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />. The registered
        /// name is resolved from the type's naming metadata and settings.</typeparam>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        public TReturn AddInitialCompositeState<TCompositeState>(CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddInitialCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>Adds an initial composite state to the current machine or region.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />.</typeparam>
        /// <param name="compositeStateName">Registered name of the composite state within the machine or region.</param>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
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
        /// <summary>Adds an initial orthogonal state to the current machine or region.</summary>
        /// <param name="orthogonalStateName">Registered name of the orthogonal state within the machine or region.</param>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized stage, not the nested orthogonal builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        TReturn AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>Adds an initial orthogonal state to the current machine or region.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />. The registered
        /// name is resolved from the type's naming metadata and settings.</typeparam>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        public TReturn AddInitialOrthogonalState<TOrthogonalState>(OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddInitialOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>Adds an initial orthogonal state to the current machine or region.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />.</typeparam>
        /// <param name="orthogonalStateName">Registered name of the orthogonal state within the machine or region.</param>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition's initialized builder stage.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
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
