namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds a terminal final node to an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.5.-Final-Node" />
    public interface IFinalBase<out TReturn>
    {
        /// <summary>Adds a final node that completes the current Activity execution.</summary>
        /// <param name="name">Optional globally unique node name; defaults to <see cref="FinalNode.Name"/>.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.5.-Final-Node" />
        TReturn AddFinal(string? name = null);
    }
}
