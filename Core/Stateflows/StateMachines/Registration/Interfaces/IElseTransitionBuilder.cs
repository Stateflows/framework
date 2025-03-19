using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseTransitionBuilder<TEvent> :
        ITransitionUtils<IElseTransitionBuilder<TEvent>>,
        IEffect<TEvent, IElseTransitionBuilder<TEvent>>
    { }
}
