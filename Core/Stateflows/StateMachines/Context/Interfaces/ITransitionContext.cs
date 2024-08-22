using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface ITransitionContext<out TEvent> : IEventActionContext<TEvent>, ITransitionContext
        where TEvent : Event, new()
    { }
}
