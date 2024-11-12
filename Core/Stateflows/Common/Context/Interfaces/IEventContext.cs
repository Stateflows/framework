namespace Stateflows.Common.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IBehaviorActionContext
    {
        TEvent Event { get; }
    }
}
