using Stateflows.Common;

namespace Stateflows.StateMachines
{
    public abstract class Transition<TEvent> : BaseTransition<TEvent>
        where TEvent : Event, new()
    { }
}
