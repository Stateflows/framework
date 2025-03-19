using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ICompositeStateExtension<out TReturn>
    {
        /// <summary>
        /// Extends state with given state type.
        /// If class implements <see cref="ICompositeStateEntry"/>, <see cref="ICompositeStateExit"/>, <see cref="ICompositeStateInitialization"/>,
        /// <see cref="ICompositeStateFinalization"/>, and/or <see cref="ICompositeStateDefinition"/>,
        /// those implementations will be added to extended state. State name will remain unchanged.
        /// </summary>
        /// <typeparam name="TCompositeState">Extending state type</typeparam>
        public TReturn ExtendWith<TCompositeState>()
            where TCompositeState : class, ICompositeState
            => ((ICompositeStateBuilder)this).AddCompositeStateEvents<TCompositeState, TReturn>();
    }
}
