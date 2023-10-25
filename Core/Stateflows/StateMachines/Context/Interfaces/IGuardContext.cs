using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IGuardContext<out TEvent> : ITransitionContext<TEvent>
        where TEvent : Event, new()
    { }
}
