using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Locates inherited states and pseudostates and extends them without creating duplicate elements.
    /// </summary>
    /// <typeparam name="TReturn">The enclosing override builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IStateMachineOverrides<out TReturn>
    {
        #region UseState
        /// <summary>Extends an inherited state without adding a new vertex.</summary>
        /// <param name="stateName">Registered name of the inherited regular state to extend.</param>
        /// <param name="stateBuildAction">Callback configuring the state's lifecycle handlers and outgoing transitions. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder, not the inherited state's callback builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseState(string stateName, OverridenStateBuildAction stateBuildAction);

        /// <summary>Extends an inherited state without adding a new vertex.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />. Its resolved state name identifies
        /// the inherited vertex to extend.</typeparam>
        /// <param name="stateBuildAction">Callback configuring the state's lifecycle handlers and outgoing transitions. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseState<TState>(OverridenStateBuildAction stateBuildAction)
            where TState : class, IState
            => UseState(State<TState>.Name, stateBuildAction);
        #endregion

        #region UseCompositeState
        /// <summary>Extends an inherited composite state without adding a new vertex.</summary>
        /// <param name="compositeStateName">Registered name of the inherited composite state to extend.</param>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction);

        /// <summary>Extends an inherited composite state without adding a new vertex.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />. Its resolved
        /// state name identifies the inherited vertex to extend.</typeparam>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseCompositeState<TCompositeState>(OverridenCompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => UseCompositeState(State<TCompositeState>.Name, compositeStateBuildAction);
        #endregion

        #region UseOrthogonalState
        /// <summary>Extends an inherited orthogonal state without adding a new vertex.</summary>
        /// <param name="orthogonalStateName">Registered name of the inherited orthogonal state to extend.</param>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>Extends an inherited orthogonal state without adding a new vertex.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />. Its resolved
        /// state name identifies the inherited vertex to extend.</typeparam>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseOrthogonalState<TOrthogonalState>(OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => UseOrthogonalState(State<TOrthogonalState>.Name, orthogonalStateBuildAction);
        #endregion

        #region UseJunction
        /// <summary>Extends an inherited junction pseudostate without adding a new vertex.</summary>
        /// <param name="junctionName">Registered name of the inherited junction pseudostate to extend.</param>
        /// <param name="junctionBuildAction">Callback configuring the junction's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction);

        /// <summary>Extends an inherited junction pseudostate without adding a new vertex.</summary>
        /// <param name="junctionBuildAction">Callback configuring the junction's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseJunction(OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(State<Junction>.Name, junctionBuildAction);
        #endregion

        #region UseChoice
        /// <summary>Extends an inherited choice pseudostate without adding a new vertex.</summary>
        /// <param name="choiceName">Registered name of the inherited choice pseudostate to extend.</param>
        /// <param name="choiceBuildAction">Callback configuring the choice's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction);

        /// <summary>Extends an inherited choice pseudostate without adding a new vertex.</summary>
        /// <param name="choiceBuildAction">Callback configuring the choice's guarded branches and fallback branch. Its
        /// builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseChoice(OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(State<Choice>.Name, choiceBuildAction);
        #endregion

        #region UseFork
        /// <summary>Extends an inherited fork pseudostate without adding a new vertex.</summary>
        /// <param name="forkName">Registered name of the inherited fork pseudostate to extend.</param>
        /// <param name="forkBuildAction">Callback configuring the fork's outgoing paths into separate orthogonal regions.
        /// Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFork(string forkName, OverridenForkBuildAction forkBuildAction);

        /// <summary>Extends an inherited fork pseudostate without adding a new vertex.</summary>
        /// <param name="forkBuildAction">Callback configuring the fork's outgoing paths into separate orthogonal regions.
        /// Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFork(OverridenForkBuildAction forkBuildAction)
            => UseFork(State<Fork>.Name, forkBuildAction);
        #endregion

        #region UseJoin
        /// <summary>Extends an inherited join pseudostate without adding a new vertex.</summary>
        /// <param name="joinName">Registered name of the inherited join pseudostate to extend.</param>
        /// <param name="joinBuildAction">Callback configuring the join's outgoing continuation. Its builder is scoped to
        /// the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction);

        /// <summary>Extends an inherited join pseudostate without adding a new vertex.</summary>
        /// <param name="joinBuildAction">Callback configuring the join's outgoing continuation. Its builder is scoped to
        /// the element, not the enclosing machine or region.</param>
        /// <returns>The enclosing override builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseJoin(OverridenJoinBuildAction joinBuildAction)
            => UseJoin(State<Join>.Name, joinBuildAction);
        #endregion
    }
}
