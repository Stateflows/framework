using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Registration.Interfaces
{
    public interface IForwardedEventBuilder<TEvent> :
        IGuard<TEvent, IForwardedEventBuilder<TEvent>>
        where TEvent : Event, new()
    { }
}
