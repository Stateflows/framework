namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ITransitionUtils<out TReturn>
    {
        /// <summary>
        /// Sets flag value for polymorphic triggers support in the Transition.<br/>
        /// Default value of the flag is false unless Transition is triggered by an Exception descendant.
        /// </summary>
        /// <param name="polymorphicTriggers">Value of the polymorphic triggers flag.</param>
        TReturn SetPolymorphicTriggers(bool polymorphicTriggers);

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
