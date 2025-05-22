namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ITargetedTransitionUtils<out TReturn>
    {
        /// <summary>
        /// Sets Transition's IsLocal property.<br/>
        /// Local and external Transitions are
        /// <a href="https://github.com/Stateflows/framework/wiki/Evaluation-of-Transitions#local-transitions">
        /// evaluated differently in certain circumstances</a>.<br/>
        /// All transitions in Stateflows State Machines are local by default.
        /// </summary>
        /// <param name="isLocal"></param>
        TReturn SetIsLocal(bool isLocal);
    }
}
