namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds a terminal final node to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-final-node" />
    public interface IFinalBase<out TReturn>
    {
        /// <summary>Adds a final node that completes the current Activity execution.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-final-node" />
        TReturn AddFinal();
    }
}
