namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds the entry point of an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
    public interface IInitialBase<out TReturn>
    {
        /// <summary>Adds the Activity's initial node.</summary>
        /// <param name="buildAction">Callback configuring the initial node's outgoing flows. Its builder is scoped to the
        /// node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-initial-node" />
        TReturn AddInitial(InitialBuildAction buildAction);
    }
}
