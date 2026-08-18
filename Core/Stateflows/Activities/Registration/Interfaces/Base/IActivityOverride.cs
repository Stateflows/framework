namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityOverride<out TReturn>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TActivity">Base activity to be overriden</typeparam>
        /// <param name="buildAction">Overriden activity build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseActivity<TActivity>(OverridenActivityBuildAction buildAction)
            where TActivity : class, IActivity;
    }
}
