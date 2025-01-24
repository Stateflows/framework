namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateComposition<out TReturn>
    {
        /// <summary>
        /// Makes current state composite.<br/>
        /// <a href="https://github.com/Stateflows/framework/wiki/Composite-State">Composite state</a> represent a stable configuration of a <a href="https://github.com/Stateflows/framework/wiki/Behaviors">Behavior</a>, which is a parent for a set of substates.
        /// </summary>
        /// <param name="compositeStateBuildAction">Composite state build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn MakeComposite(CompositeStateBuildAction compositeStateBuildAction);
    }
}
