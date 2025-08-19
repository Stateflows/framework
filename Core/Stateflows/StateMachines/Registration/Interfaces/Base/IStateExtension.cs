using System.Diagnostics;
using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateExtension<out TReturn>
    {
        /// <summary>
        /// Extends state with given state type.
        /// If class implements <see cref="IStateEntry"/>, <see cref="IStateExit"/>, and/or <see cref="IStateDefinition"/>,
        /// those implementations will be added to extended state. State name will remain unchanged.
        /// </summary>
        /// <typeparam name="TState">Extending state type</typeparam>
        [DebuggerHidden]
        public TReturn ExtendWith<TState>()
            where TState : class, IState
            => ((IStateBuilder)this).AddStateEvents<TState, TReturn>();
    }
}
