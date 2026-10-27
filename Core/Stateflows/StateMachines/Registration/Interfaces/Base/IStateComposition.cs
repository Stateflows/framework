namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Converts the current regular state into a composite state with one nested region.</summary>
    /// <typeparam name="TReturn">The builder stage returned after conversion.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateComposition<out TReturn>
    {
        /// <summary>Converts the current state into a composite state with a nested region.</summary>
        /// <param name="compositeStateBuildAction">Callback configuring the composite state's lifecycle, nested states, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The builder stage for continuing configuration after composition.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        TReturn MakeComposite(CompositeStateBuildAction compositeStateBuildAction);
    }
}
