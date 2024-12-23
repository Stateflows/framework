namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IPseudostateTransitions<out TReturn>
    {
        /// <summary>
        /// Adds default transition coming from current pseudostate.<br/>
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
        TReturn AddTransition(string targetStateName, DefaultTransitionBuildAction transitionBuildAction = null);
    }
    
    public interface IPseudostateElseTransitions<out TReturn> : IPseudostateTransitions<TReturn>
    {
        /// <summary>
        /// Adds else alternative for all default transitions coming from current pseudostate.<br/><br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        void AddElseTransition(string targetStateName, ElseDefaultTransitionBuildAction transitionBuildAction = null);
    }
}
