namespace Stateflows.Common.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IBehaviorActionContext
        where TEvent : Event
    {
        TEvent Event { get; }
    }
}
