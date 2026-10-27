namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Converts the current state into an orthogonal state, retaining existing nested contents in its first
    /// region.</summary>
    /// <typeparam name="TReturn">The builder stage returned after conversion.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public interface IStateOrthogonalization<out TReturn>
    {
        /// <summary>Converts the current state into an orthogonal state with independently active regions.</summary>
        /// <param name="orthogonalStateBuildAction">Callback configuring the orthogonal state's lifecycle, regions, and
        /// outgoing transitions. Its builder is scoped to the element, not the enclosing machine or region.</param>
        /// <returns>The builder stage for continuing configuration after regionalization.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        TReturn MakeOrthogonal(OrthogonalStateBuildAction orthogonalStateBuildAction);
    }
}
