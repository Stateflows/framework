using Stateflows.Common;

namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface ITransitionContext<out TEvent> : IEventContext<TEvent>
        where TEvent : Event
    {
        IStateContext SourceState { get; }

        IStateContext TargetState { get; }
    }
}
