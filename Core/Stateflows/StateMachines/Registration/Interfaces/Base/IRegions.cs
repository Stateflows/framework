namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IRegions<out TReturn>
    {
        /// <summary>
        /// Adds a <a href="https://github.com/Stateflows/framework/wiki/Regions">region</a> to the current orthogonal state.
        /// </summary>
        /// <param name="buildAction">Region build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn AddRegion(RegionBuildAction buildAction);
    }
}

