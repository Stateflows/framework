namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Reuses a typed machine definition as the basis for a new fluent definition.</summary>
    /// <typeparam name="TReturn">The builder stage exposing inherited elements for extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IStateMachineOverride<out TReturn>
    {
        /// <summary>Copies a typed State Machine definition into this registration and extends its inherited elements.
        /// </summary>
        /// <typeparam name="TStateMachine">Base State Machine implementation whose definition is copied before inherited
        /// elements are extended.</typeparam>
        /// <param name="buildAction">Callback extending elements copied from the typed base definition.</param>
        /// <returns>The override builder stage exposing inherited elements for extension.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            where TStateMachine : class, IStateMachine;
    }
}
