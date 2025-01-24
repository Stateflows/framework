namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateOrthogonalization<out TReturn>
    {
        /// <summary>
        /// Makes current state orthogonal.<br/>
        /// If converted state is <a href="https://github.com/Stateflows/framework/wiki/Composite-State">composite state</a>, its contents will end up in first <a href="https://github.com/Stateflows/framework/wiki/Regions">region</a> of resulting orthogonal state.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Orthogonal-State">Orthogonal state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of orthogonal (parallel) <a href="https://github.com/Stateflows/framework/wiki/Regions">regions</a>.
        /// </summary>
        /// <param name="orthogonalStateBuildAction">Orthogonal state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction);
    }
}
