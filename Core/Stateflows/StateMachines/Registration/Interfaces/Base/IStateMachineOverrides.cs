namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineOverrides<out TReturn>
    {
        /// <summary>
        /// Uses a State of overriden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        TReturn UseState(string stateName, OverridenStateBuildAction stateBuildAction);

        /// <summary>
        /// Uses a Composite State of overriden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        TReturn UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction);

        /// <summary>
        /// Uses a Orthogonal State of overriden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for set of substates.
        /// </summary>
        /// <param name="orthogonalStateName">Orthogonal state name</param>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action</param>
        TReturn UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction);

        /// <summary>
        /// Uses a Junction pseudostate of overriden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionName">Junction name</param>
        /// <param name="junctionBuildAction">Junction build action</param>
        TReturn UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction);

        /// <summary>
        /// Uses a Junction pseudostate of overriden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionBuildAction">Junction build action</param>
        TReturn UseJunction(OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(Junction.Name, junctionBuildAction);

        /// <summary>
        /// Uses a Choice pseudostate of overriden state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceName">Choice name</param>
        /// <param name="choiceBuildAction">Choice build action</param>
        TReturn UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction);

        /// <summary>
        /// Uses a Choice pseudostate of overriden state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceBuildAction">Choice build action</param>
        TReturn UseChoice(OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(Choice.Name, choiceBuildAction);
        
        TReturn UseFork(string forkName, OverridenForkBuildAction forkBuildAction);
        
        TReturn UseFork(OverridenForkBuildAction forkBuildAction)
            => UseFork(Fork.Name, forkBuildAction);
        
        TReturn UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction);
        
        TReturn UseJoin(OverridenJoinBuildAction joinBuildAction)
            => UseJoin(Join.Name, joinBuildAction);
    }
}
