namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Adds independent regions to the current orthogonal state.</summary>
    /// <typeparam name="TReturn">The enclosing orthogonal-state builder returned after region definition.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IRegions<out TReturn>
    {
        /// <summary>Adds an independently active region to the current orthogonal state.</summary>
        /// <param name="buildAction">Callback configuring the region's initial state, vertices, and transitions.</param>
        /// <returns>The enclosing orthogonal-state builder, not the region-scoped callback builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
        TReturn AddRegion(RegionBuildAction buildAction);
    }
}
