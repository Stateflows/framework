using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineOverrides<out TReturn>
    {
        #region UseState
        /// <summary>
        /// Uses a state of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">States</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseState(string stateName, OverridenStateBuildAction stateBuildAction);

        /// <summary>
        /// Uses a state of the overridden state machine.<br/>
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
        public TReturn UseState<TState>(OverridenStateBuildAction stateBuildAction = null)
            where TState : class, IState
            => UseState(State<TState>.Name, stateBuildAction);
        #endregion

        #region UseCompositeState
        /// <summary>
        /// Uses a composite state of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction);

        /// <summary>
        /// Uses a composite state of the overridden state machine.<br/>
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
        public TReturn UseCompositeState<TCompositeState>(OverridenCompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => UseCompositeState(State<TCompositeState>.Name, compositeStateBuildAction);
        #endregion

        #region UseOrthogonalState
        /// <summary>
        /// Uses an orthogonal state of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal states</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>
        /// Uses an orthogonal state of the overridden state machine.<br/>
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
        public TReturn UseOrthogonalState<TOrthogonalState>(OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => UseOrthogonalState(State<TOrthogonalState>.Name, orthogonalStateBuildAction);
        #endregion

        #region UseJunction
        /// <summary>
        /// Uses a junction pseudostate of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionName">Junction name</param>
        /// <param name="junctionBuildAction">Junction build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction);

        /// <summary>
        /// Uses a junction pseudostate of the overridden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionBuildAction">Junction build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseJunction(OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(Junction.Name, junctionBuildAction);
        #endregion

        #region UseChoice
        /// <summary>
        /// Uses a choice pseudostate of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceName">Choice name</param>
        /// <param name="choiceBuildAction">Choice build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction);

        /// <summary>
        /// Uses a choice pseudostate of the overridden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceBuildAction">Choice build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseChoice(OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(Choice.Name, choiceBuildAction);
        #endregion

        #region UseFork
        /// <summary>
        /// Uses a fork pseudostate of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Fork">Forks</a> are pseudostates used to split a single transition into multiple concurrent transitions.
        /// </summary>
        /// <param name="forkName">Fork name</param>
        /// <param name="forkBuildAction">Fork build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseFork(string forkName, OverridenForkBuildAction forkBuildAction);

        /// <summary>
        /// Uses a fork pseudostate of the overridden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Fork">Forks</a> are pseudostates used to split a single transition into multiple concurrent transitions.
        /// </summary>
        /// <param name="forkBuildAction">Fork build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseFork(OverridenForkBuildAction forkBuildAction)
            => UseFork(Fork.Name, forkBuildAction);
        #endregion

        #region UseJoin
        /// <summary>
        /// Uses a join pseudostate of the overridden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Join">Joins</a> are pseudostates used to merge multiple concurrent transitions into a single transition.
        /// </summary>
        /// <param name="joinName">Join name</param>
        /// <param name="joinBuildAction">Join build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction);

        /// <summary>
        /// Uses a join pseudostate of the overridden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Join">Joins</a> are pseudostates used to merge multiple concurrent transitions into a single transition.
        /// </summary>
        /// <param name="joinBuildAction">Join build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseJoin(OverridenJoinBuildAction joinBuildAction)
            => UseJoin(Join.Name, joinBuildAction);
        #endregion
    }
}
