namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IRegionsOverrides<out TReturn>
    {
        /// <summary>
        /// Uses existing <a href="https://github.com/Stateflows/framework/wiki/Regions">region</a> with given index in current orthogonal state.
        /// </summary>
        /// <param name="index">Index of existing region</param>
        /// <param name="buildAction">Region build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseRegion(int index, OverridenRegionBuildAction buildAction);
    }
}
