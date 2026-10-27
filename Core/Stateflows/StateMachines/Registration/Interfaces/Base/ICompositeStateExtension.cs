using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Applies a typed composite-state definition to the current builder.</summary>
    /// <typeparam name="TReturn">The fluent builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
    public interface ICompositeStateExtension<out TReturn>
    {
        /// <summary>Applies a typed state definition's supported hooks without changing the current state's name.</summary>
        /// <typeparam name="TCompositeState">Typed state implementation of <see cref="ICompositeState" />.</typeparam>
        /// <returns>The current composite-state builder after applying the typed definition.</returns>
        /// <remarks>Supported entry, exit, and definition hooks are applied to the existing state; its registered name is
        /// preserved.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-composite-state" />
        public TReturn ExtendWith<TCompositeState>()
            where TCompositeState : class, ICompositeState
            => ((ICompositeStateBuilder)this).AddCompositeStateEvents<TCompositeState, TReturn>();
    }
}
