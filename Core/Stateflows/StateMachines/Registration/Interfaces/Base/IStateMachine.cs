using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachine<out TReturn>
    {
        #region AddState
        /// <summary>
        /// Adds a State to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        TReturn AddState(string stateName, StateBuildAction stateBuildAction = null);
        /// <summary>
        /// Adds state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateBuildAction">State build action</param>
        [DebuggerHidden]
        public TReturn AddState<TState>(StateBuildAction stateBuildAction = null)
            where TState : class, IState
            => AddState<TState>(State<TState>.Name, stateBuildAction);

        /// <summary>
        /// Adds state to current composite state.
        /// </summary>
        /// <typeparam name="TState">State class; must implement at least one of following interfaces: <see cref="IStateEntry"/>, <see cref="IStateExit"/></typeparam>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        /// <returns></returns>
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
        /// Adds a Composite State to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        TReturn AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);
        
        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        [DebuggerHidden]
        public TReturn AddCompositeState<TCompositeState>(CompositeStateBuildAction compositeStateBuildAction)
            where TCompositeState : class, ICompositeState
            => AddCompositeState<TCompositeState>(State<TCompositeState>.Name, compositeStateBuildAction);

        /// <summary>
        /// Adds initial composite state to current composite state.
        /// </summary>
        /// <typeparam name="TCompositeState">Composite state class; must implement at least one of following interfaces: <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>, <see cref="ICompositeStateFinalization"/></typeparam>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
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
        /// Adds a Orthogonal State to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for set of substates.
        /// </summary>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
        TReturn AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction);
        /// <summary>
        /// Adds initial orthogonal state to current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>

        [DebuggerHidden]
        public TReturn AddOrthogonalState<TOrthogonalState>(OrthogonalStateBuildAction orthogonalStateBuildAction)
            where TOrthogonalState : class, IOrthogonalState
            => AddOrthogonalState<TOrthogonalState>(State<TOrthogonalState>.Name, orthogonalStateBuildAction);

        /// <summary>
        /// Adds initial orthogonal state to current orthogonal state.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Orthogonal state class; must implement at least one of following interfaces: <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>, <see cref="IOrthogonalStateFinalization"/></typeparam>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
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
        
        /// <summary>
        /// Adds a Junction pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionName">Junction name</param>
        /// <param name="junctionBuildAction">Junction build action</param>
        TReturn AddJunction(string junctionName, JunctionBuildAction junctionBuildAction);

        /// <summary>
        /// Adds a Junction pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionBuildAction">Junction build action</param>
        TReturn AddJunction(JunctionBuildAction junctionBuildAction)
            => AddJunction(Junction.Name, junctionBuildAction);

        /// <summary>
        /// Adds a Choice pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceName">Choice name</param>
        /// <param name="choiceBuildAction">Choice build action</param>
        TReturn AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction);

        /// <summary>
        /// Adds a Choice pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceBuildAction">Choice build action</param>
        TReturn AddChoice(ChoiceBuildAction choiceBuildAction)
            => AddChoice(Choice.Name, choiceBuildAction);
        
        TReturn AddFork(string forkName, ForkBuildAction forkBuildAction);
        
        TReturn AddFork(ForkBuildAction forkBuildAction)
            => AddFork(Fork.Name, forkBuildAction);
        
        TReturn AddJoin(string joinName, JoinBuildAction joinBuildAction);
        
        TReturn AddJoin(JoinBuildAction joinBuildAction)
            => AddJoin(Join.Name, joinBuildAction);
    }
}
