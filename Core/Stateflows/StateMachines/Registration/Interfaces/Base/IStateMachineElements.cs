using System.Diagnostics;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds states and pseudostates to the current machine or nested region.</summary>
    /// <typeparam name="TReturn">The enclosing definition builder returned after adding an element.</typeparam>
    /// <remarks>Element callbacks configure their own scoped builders; the returned chain remains at the enclosing
    /// level. See <see href="https://github.com/Stateflows/framework/wiki/state-machine-state">states</see>.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machines" />
    public interface IStateMachineElements<out TReturn>
    {
        #region AddState
        /// <summary>Adds a state to the current machine or region.</summary>
        /// <param name="stateName">Registered name of the regular state within the machine or region.</param>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder, not the state-scoped callback builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        TReturn AddState(string stateName, StateBuildAction stateBuildAction = null);

        /// <summary>Adds a state to the current machine or region.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />. The registered name is resolved
        /// from the type's naming metadata and settings.</typeparam>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn AddState<TState>(StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>Adds a state to the current machine or region.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />.</typeparam>
        /// <param name="stateName">Registered name of the regular state within the machine or region.</param>
        /// <param name="stateBuildAction">Optional callback configuring the state's lifecycle handlers and outgoing
        /// transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn AddState<TState>(string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
        {
            var result = AddState(
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

        #region AddCompositeState
        /// <summary>Adds a composite state to the current machine or region.</summary>
        /// <param name="compositeStateName">Registered name of the composite state within the machine or region.</param>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder, not the composite-state callback builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        /// <summary>Adds a composite state to the current machine or region.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />. The registered
        /// name is resolved from the type's naming metadata and settings.</typeparam>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        public TReturn AddCompositeState<TCompositeState>(CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>Adds a composite state to the current machine or region.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />.</typeparam>
        /// <param name="compositeStateName">Registered name of the composite state within the machine or region.</param>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        [DebuggerHidden]
        public TReturn AddCompositeState<TCompositeState>(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
        {
            var result = AddCompositeState(
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

        #region AddOrthogonalState
        /// <summary>Adds an orthogonal state to the current machine or region.</summary>
        /// <param name="orthogonalStateName">Registered name of the orthogonal state within the machine or region.</param>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder, not the orthogonal-state callback builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        TReturn AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>Adds an orthogonal state to the current machine or region.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />. The registered
        /// name is resolved from the type's naming metadata and settings.</typeparam>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        public TReturn AddOrthogonalState<TOrthogonalState>(OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>Adds an orthogonal state to the current machine or region.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />.</typeparam>
        /// <param name="orthogonalStateName">Registered name of the orthogonal state within the machine or region.</param>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        [DebuggerHidden]
        public TReturn AddOrthogonalState<TOrthogonalState>(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
        {
            var result = AddOrthogonalState(
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

        #region AddJunction
        /// <summary>Adds a junction pseudostate to the current machine or region.</summary>
        /// <param name="junctionName">Registered name of the junction pseudostate within the machine or region.</param>
        /// <param name="junctionBuildAction">Callback configuring the junction's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-junction" />
        TReturn AddJunction(string junctionName, JunctionBuildAction junctionBuildAction);

        /// <summary>Adds a junction pseudostate to the current machine or region.</summary>
        /// <param name="junctionBuildAction">Callback configuring the junction's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-junction" />
        TReturn AddJunction(JunctionBuildAction junctionBuildAction)
            => AddJunction(State<Junction>.Name, junctionBuildAction);
        #endregion

        #region AddChoice
        /// <summary>Adds a choice pseudostate to the current machine or region.</summary>
        /// <param name="choiceName">Registered name of the choice pseudostate within the machine or region.</param>
        /// <param name="choiceBuildAction">Callback configuring the choice's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-choice" />
        TReturn AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction);

        /// <summary>Adds a choice pseudostate to the current machine or region.</summary>
        /// <param name="choiceBuildAction">Callback configuring the choice's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-choice" />
        TReturn AddChoice(ChoiceBuildAction choiceBuildAction)
            => AddChoice(State<Choice>.Name, choiceBuildAction);
        #endregion

        #region AddFork
        /// <summary>Adds a fork pseudostate to the current machine or region.</summary>
        /// <param name="forkName">Registered name of the fork pseudostate within the machine or region.</param>
        /// <param name="forkBuildAction">Callback configuring the fork's outgoing paths into separate orthogonal regions.
        /// Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
        TReturn AddFork(string forkName, ForkBuildAction forkBuildAction);

        /// <summary>Adds a fork pseudostate to the current machine or region.</summary>
        /// <param name="forkBuildAction">Callback configuring the fork's outgoing paths into separate orthogonal regions.
        /// Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-fork" />
        TReturn AddFork(ForkBuildAction forkBuildAction)
            => AddFork(State<Fork>.Name, forkBuildAction);
        #endregion

        #region AddJoin
        /// <summary>Adds a join pseudostate to the current machine or region.</summary>
        /// <param name="joinName">Registered name of the join pseudostate within the machine or region.</param>
        /// <param name="joinBuildAction">Callback configuring the join's outgoing continuation. Its builder is scoped to
        /// the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
        TReturn AddJoin(string joinName, JoinBuildAction joinBuildAction);

        /// <summary>Adds a join pseudostate to the current machine or region.</summary>
        /// <param name="joinBuildAction">Callback configuring the join's outgoing continuation. Its builder is scoped to
        /// the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing definition builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-join" />
        TReturn AddJoin(JoinBuildAction joinBuildAction)
            => AddJoin(State<Join>.Name, joinBuildAction);
        #endregion
    }
}
