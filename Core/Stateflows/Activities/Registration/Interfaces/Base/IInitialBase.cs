namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds the entry point of an activity graph.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.4.-Initial-Node" />
    public interface IInitialBase<out TReturn>
    {
        /// <summary>Adds the Activity's initial node, named <c>{parentName}.initial</c>.</summary>
        /// <param name="buildAction">Callback configuring the initial node's outgoing flows. Its builder is scoped to the
        /// node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.4.-Initial-Node" />
        TReturn AddInitial(InitialBuildAction buildAction);
    }
}
