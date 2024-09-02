using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitions<TReturn>
    {
        #region Transitions
        /// <summary>
        /// Adds transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Target</term>
        /// <description>Name of the state that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>second parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>
        /// Adds default transition coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>second parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>
        /// Adds internal transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that is accepted by transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion

        #region ElseTransitions
        /// <summary>
        /// Adds else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction = null);

        /// <summary>
        /// Adds else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null);

        /// <summary>
        /// Adds internal else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://www.stateflows.net/documentation/definition#transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion
    }
}
