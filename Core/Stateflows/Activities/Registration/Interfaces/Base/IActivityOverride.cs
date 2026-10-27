namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Selects an inherited activity definition to override.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IActivityOverride<out TReturn>
    {
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TActivity">Activity node implementation implementing <see cref="IActivity" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseActivity<TActivity>(OverridenActivityBuildAction buildAction)
            where TActivity : class, IActivity;
    }
}
