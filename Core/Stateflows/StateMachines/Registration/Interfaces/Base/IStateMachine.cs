using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachine<out TReturn>
    {
        #region AddState
        /// <summary>
        /// Adds a state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddState(string stateName, StateBuildAction stateBuildAction = null);

        /// <summary>
        /// Adds a state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
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
        public TReturn AddState<TState>(StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds a state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
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
        public TReturn AddState<TState>(string stateName, StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddState(
                stateName,
                b =>
                {
                    b.AddStateEvents<TState, IStateBuilder>();

                    stateBuildAction?.Invoke(b);
                }
            );
        #endregion

        #region AddCompositeState
        /// <summary>
        /// Adds a composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        /// <summary>
        /// Adds an composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
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
        public TReturn AddCompositeState<TCompositeState>(CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds a composite state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
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
        public TReturn AddCompositeState<TCompositeState>(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddCompositeState(
                compositeStateName,
                b =>
                {
                    b.AddStateEvents<TCompositeState, ICompositeStateBuilder>();
                    b.AddCompositeStateEvents<TCompositeState, ICompositeStateBuilder>();

                    compositeStateBuildAction?.Invoke(b);
                }
            );
        #endregion

        #region AddOrthogonalState
        /// <summary>
        /// Adds a orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>
        /// Adds a orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
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
        public TReturn AddOrthogonalState<TOrthogonalState>(OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>
        /// Adds a orthogonal state to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
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
        public TReturn AddOrthogonalState<TOrthogonalState>(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddOrthogonalState(
                orthogonalStateName,
                b =>
                {
                    b.AddStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();
                    b.AddCompositeStateEvents<TOrthogonalState, IOrthogonalStateBuilder>();

                    orthogonalStateBuildAction?.Invoke(b);
                }
            );
        #endregion

        #region AddJunction
        /// <summary>
        /// Adds a junction pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionName">Junction name</param>
        /// <param name="junctionBuildAction">Junction build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddJunction(string junctionName, JunctionBuildAction junctionBuildAction);

        /// <summary>
        /// Adds a junction pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionBuildAction">Junction build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddJunction(JunctionBuildAction junctionBuildAction)
            => AddJunction(Junction.Name, junctionBuildAction);
        #endregion

        #region AddChoice
        /// <summary>
        /// Adds a choice pseudostate to the state machine.<br/>
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
        TReturn AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction);

        /// <summary>
        /// Adds a choice pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceBuildAction">Choice build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddChoice(ChoiceBuildAction choiceBuildAction)
            => AddChoice(Choice.Name, choiceBuildAction);
        #endregion

        #region AddFork
        /// <summary>
        /// Adds a fork pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Fork">Forks</a> are pseudostates used to split a single transition into multiple concurrent transitions.
        /// </summary>
        /// <param name="forkName">Fork name</param>
        /// <param name="forkBuildAction">Fork build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddFork(string forkName, ForkBuildAction forkBuildAction);

        /// <summary>
        /// Adds a fork pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Fork">Forks</a> are pseudostates used to split a single transition into multiple concurrent transitions.
        /// </summary>
        /// <param name="forkBuildAction">Fork build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddFork(ForkBuildAction forkBuildAction)
            => AddFork(Fork.Name, forkBuildAction);
        #endregion

        #region AddJoin
        /// <summary>
        /// Adds a join pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Join">Joins</a> are pseudostates used to merge multiple concurrent transitions into a single transition.
        /// </summary>
        /// <param name="joinName">Join name</param>
        /// <param name="joinBuildAction">Join build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddJoin(string joinName, JoinBuildAction joinBuildAction);

        /// <summary>
        /// Adds a join pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Join">Joins</a> are pseudostates used to merge multiple concurrent transitions into a single transition.
        /// </summary>
        /// <param name="joinBuildAction">Join build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddJoin(JoinBuildAction joinBuildAction)
            => AddJoin(Join.Name, joinBuildAction);
        #endregion
    }
}
