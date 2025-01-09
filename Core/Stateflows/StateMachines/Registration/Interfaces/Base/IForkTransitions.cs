namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IForkTransitions<out TReturn>
    {
        /// <summary>
        /// Adds default transition coming from current fork pseudostate.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Default-Transition">Default transitions</a> are triggered automatically after every State Machine execution and are changing its state.
        /// <list type="number">
        /// <item>
        /// <term>Target</term>
        /// <description>State that transition is coming into - <b>first parameter</b>,</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="targetStateName">Target state name</param>
        /// <param name="transitionBuildAction">Transition build action</param>
        TReturn AddTransition(string targetStateName, DefaultTransitionEffectBuildAction transitionBuildAction = null);
    }
}
