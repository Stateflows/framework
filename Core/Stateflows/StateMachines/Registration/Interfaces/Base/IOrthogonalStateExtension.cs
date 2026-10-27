using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Applies a typed orthogonal-state definition to the current state.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
    public interface IOrthogonalStateExtension<out TReturn>
    {
        /// <summary>Applies a typed state definition's supported hooks without changing the current state's name.</summary>
        /// <typeparam name="TOrthogonalState">Typed state implementation of <see cref="IOrthogonalState" />.</typeparam>
        /// <returns>The current orthogonal-state builder after applying the typed definition.</returns>
        /// <remarks>Supported entry, exit, and definition hooks are applied to the existing state; its registered name is
        /// preserved.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-orthogonal-state" />
        public TReturn ExtendWith<TOrthogonalState>()
            where TOrthogonalState : class, IOrthogonalState
            => ((IOrthogonalStateBuilder)this).AddOrthogonalStateEvents<TOrthogonalState, TReturn>();
    }
}
