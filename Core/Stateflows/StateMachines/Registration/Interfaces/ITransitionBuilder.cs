using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface ITransitionBuilder<TEvent> :
        IEffect<TEvent, ITransitionBuilder<TEvent>>,
        IGuard<TEvent, ITransitionBuilder<TEvent>>
    { }
}
