using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IElseInternalTransitionBuilder<TEvent> :
        IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>
        where TEvent : Event, new()
    { }
}
