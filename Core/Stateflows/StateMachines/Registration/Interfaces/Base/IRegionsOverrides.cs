namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Extends a region inherited from a reused orthogonal-state definition.</summary>
    /// <typeparam name="TReturn">The enclosing state builder returned after region extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
    public interface IRegionsOverrides<out TReturn>
    {
        /// <summary>Extends an inherited region of the current orthogonal state.</summary>
        /// <param name="index">Zero-based index of the inherited region in the current orthogonal state.</param>
        /// <param name="buildAction">Callback configuring the inherited region's initial state, vertices, and transitions.
        /// </param>
        /// <returns>The enclosing orthogonal-state builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-regions" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseRegion(int index, OverridenRegionBuildAction buildAction);
    }
}
