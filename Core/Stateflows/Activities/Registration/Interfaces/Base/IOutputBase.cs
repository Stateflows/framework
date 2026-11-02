namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds an output node to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-output-node" />
    public interface IOutputBase<out TReturn>
    {
        /// <summary>Adds an output boundary node to a structured Activity.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-output-node" />
        [System.Obsolete("Use AddOutputPin<TToken> to declare supported output token types.")]
        TReturn AddOutput();

        /// <summary>Adds a typed output pin, targeted with OutputPin&lt;TToken&gt;.</summary>
        TReturn AddOutputPin<TToken>();
    }
}
