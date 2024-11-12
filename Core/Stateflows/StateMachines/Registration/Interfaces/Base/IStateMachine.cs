namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public partial interface IStateMachine<out TReturn>
    {
        /// <summary>
        /// Adds a State to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/State">State</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>.
        /// </summary>
        /// <param name="stateName">State name</param>
        /// <param name="stateBuildAction">State build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddState(string stateName, StateBuildAction stateBuildAction = null);

        /// <summary>
        /// Adds a Composite State to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for set of substates.
        /// </summary>
        /// <param name="compositeStateName">Composite state name</param>
        /// <param name="compositeStateBuildAction">Composite state build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction);

        /// <summary>
        /// Adds a Junction pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionName">Junction name</param>
        /// <param name="junctionBuildAction">Junction build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddJunction(string junctionName, JunctionBuildAction junctionBuildAction);

        /// <summary>
        /// Adds a Junction pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Junction">Junctions</a> are pseudostates used to statically combine multiple transition guards; guards of transitions incoming to Junction and outgoing from Junction are evaluated together, <i>before</i> any transition happens.
        /// </summary>
        /// <param name="junctionBuildAction">Junction build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddJunction(JunctionBuildAction junctionBuildAction)
            => AddJunction(Junction.Name, junctionBuildAction);

        /// <summary>
        /// Adds a Choice pseudostate to the state machine.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceName">Choice name</param>
        /// <param name="choiceBuildAction">Choice build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction);

        /// <summary>
        /// Adds a Choice pseudostate to the state machine with a default name.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Choice">Choices</a> are pseudostates used to dynamically combine multiple transition guards; guards of transitions outgoing from Choice are evaluated <i>after</i> incoming transition happens.<br/>
        /// It is mandatory for a Choice to define exactly one else transition to make sure that there is always an available target.
        /// </summary>
        /// <param name="choiceBuildAction">Choice build action</param>
        /// <returns>Returns the state machine instance</returns>
        TReturn AddChoice(ChoiceBuildAction choiceBuildAction)
            => AddChoice(Choice.Name, choiceBuildAction);
    }
}
