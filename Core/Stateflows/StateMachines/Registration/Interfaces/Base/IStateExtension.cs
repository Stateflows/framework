using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Applies a typed regular-state definition to the current state.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after extension.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateExtension<out TReturn>
    {
        /// <summary>Applies a typed state definition's supported hooks without changing the current state's name.</summary>
        /// <typeparam name="TState">Typed state implementation of <see cref="IState" />.</typeparam>
        /// <returns>The current state builder after applying the typed definition.</returns>
        /// <remarks>Supported entry, exit, and definition hooks are applied to the existing state; its registered name is
        /// preserved.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
        [DebuggerHidden]
        public TReturn ExtendWith<TState>()
            where TState : class, IState
            => ((IStateBuilder)this).AddStateEvents<TState, TReturn>();
    }
}
