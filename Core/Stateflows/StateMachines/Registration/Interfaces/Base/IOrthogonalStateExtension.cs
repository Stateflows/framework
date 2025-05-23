using Stateflows.StateMachines.Extensions;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IOrthogonalStateExtension<out TReturn>
    {
        /// <summary>
        /// Extends state with given state type.
        /// If class implements <see cref="IOrthogonalStateEntry"/>, <see cref="IOrthogonalStateExit"/>, <see cref="IOrthogonalStateInitialization"/>,
        /// <see cref="IOrthogonalStateFinalization"/>, and/or <see cref="IOrthogonalStateDefinition"/>,
        /// those implementations will be added to extended state. State name will remain unchanged.
        /// </summary>
        /// <typeparam name="TOrthogonalState">Extending state type</typeparam>
        public TReturn ExtendWith<TOrthogonalState>()
            where TOrthogonalState : class, IOrthogonalState
            => ((IOrthogonalStateBuilder)this).AddOrthogonalStateEvents<TOrthogonalState, TReturn>();
    }
}
