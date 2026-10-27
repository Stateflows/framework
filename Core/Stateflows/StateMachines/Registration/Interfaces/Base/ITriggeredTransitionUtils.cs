namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Identifies the utility capability of an event-triggered transition builder.</summary>
    /// <typeparam name="TReturn">The fluent transition builder type.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transition" />
    public interface ITriggeredTransitionUtils<out TReturn>
    {
        // /// <summary>
        // /// Sets flag value for polymorphic triggers support in the Transition.<br/>
        // /// Default value of the flag is false unless Transition is triggered by an Exception descendant.
        // /// </summary>
        // /// <param name="polymorphicTriggers">Value of the polymorphic triggers flag.</param>
        // TReturn SetPolymorphicTriggers(bool polymorphicTriggers);
    }
}
