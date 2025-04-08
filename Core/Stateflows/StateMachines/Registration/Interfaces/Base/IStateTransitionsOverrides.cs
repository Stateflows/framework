using System.Diagnostics;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>
    /// Interface for defining state transition overrides.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the state transition override action.</typeparam>
    public interface IStateTransitionsOverrides<TReturn>
    {
        #region Transitions
        /// <summary>
        /// Uses transition triggered by TEvent coming from current state.<br/>
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
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseTransition<TEvent>(string targetStateName, OverridenTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>
        /// Uses transition triggered by TEvent coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn UseTransition<TEvent, TTargetState>(OverridenTransitionBuildAction<TEvent> transitionBuildAction)
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
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseDefaultTransition(string targetStateName, OverridenDefaultTransitionBuildAction transitionBuildAction);

        /// <summary>
        /// Uses default transition coming from current state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn UseDefaultTransition<TTargetState>(OverridenDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses internal transition triggered by TEvent coming from current state.<br/>
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
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseInternalTransition<TEvent>(OverridenInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion

        #region ElseTransitions
        /// <summary>
        /// Uses else alternative for all TEvent-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseElseTransition<TEvent>(string targetStateName, OverridenElseTransitionBuildAction<TEvent> transitionBuildAction);

        /// <summary>
        /// Uses else alternative for all transitions triggered by TEvent coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Transition">Transitions</a> are triggered by events sent to State Machine and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn UseElseTransition<TEvent, TTargetState>(OverridenElseTransitionBuildAction<TEvent> transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseElseDefaultTransition(string targetStateName, OverridenElseDefaultTransitionBuildAction transitionBuildAction);

        /// <summary>
        /// Uses else alternative for all default transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TTargetState">Target state class; must implement at least one of the following interfaces:
        /// <list type="bullet">
        /// <item><see cref="IState"/></item>
        /// <item><see cref="IStateEntry"/></item>
        /// <item><see cref="IStateExit"/></item>
        /// <item><see cref="ICompositeState"/></item>
        /// <item><see cref="ICompositeStateEntry"/></item>
        /// <item><see cref="ICompositeStateExit"/></item>
        /// <item><see cref="ICompositeStateInitialization"/></item>
        /// <item><see cref="ICompositeStateFinalization"/></item>
        /// <item><see cref="IOrthogonalState"/></item>
        /// <item><see cref="IOrthogonalStateEntry"/></item>
        /// <item><see cref="IOrthogonalStateExit"/></item>
        /// <item><see cref="IOrthogonalStateInitialization"/></item>
        /// <item><see cref="IOrthogonalStateFinalization"/></item>
        /// </list>
        /// </typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        [DebuggerHidden]
        public TReturn UseElseDefaultTransition<TTargetState>(OverridenElseDefaultTransitionBuildAction transitionBuildAction)
            where TTargetState : class, IVertex
            => UseElseDefaultTransition(State<TTargetState>.Name, transitionBuildAction);

        /// <summary>
        /// Uses internal else alternative for all TEvent-triggered transitions coming from current state.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <typeparam name="TEvent">Event class</typeparam>
        /// <param name="transitionBuildAction">Transition build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseElseInternalTransition<TEvent>(OverridenElseInternalTransitionBuildAction<TEvent> transitionBuildAction);
        #endregion
    }
}
