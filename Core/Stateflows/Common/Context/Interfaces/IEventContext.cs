namespace Stateflows.Common.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IBehaviorActionContext
        where TEvent : Event, new()
    {
        TEvent Event { get; }
    }
}
