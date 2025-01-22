using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateTransitionsOverrides<TReturn>
    {
        #region Transitions
        /// <summary>
        /// Uses transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
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
        TReturn UseTransition<TEvent>(string targetStateName, TransitionBuildAction<TEvent> transitionBuildAction);
        
        [DebuggerHidden]
        public TReturn UseTransition<TEvent, TTargetState>(TransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => UseTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
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
        TReturn UseDefaultTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction);
        
        [DebuggerHidden]
        public TReturn UseDefaultTransition<TTargetState>(DefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses internal transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Internal-Transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Trigger</term>
        /// <description>Event that triggers transition - <b>first type parameter</b>.</description>
        /// </item>
        /// <item>
        /// <term>Guard/Effect</term>
        /// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn UseInternalTransition<TEvent>(InternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion

        #region ElseTransitions
        /// <summary>
        /// Uses else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn UseElseTransition<TEvent>(string targetStateName, ElseTransitionBuildAction<TEvent> transitionBuildAction);
        
        [DebuggerHidden]
        public TReturn UseElseTransition<TEvent, TTargetState>(ElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn UseElseDefaultTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction);
        
        [DebuggerHidden]
        public TReturn UseElseDefaultTransition<TTargetState>(ElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses internal else alternative for all <see cref="TEvent"/>-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn UseElseInternalTransition<TEvent>(ElseInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion
    }
}
