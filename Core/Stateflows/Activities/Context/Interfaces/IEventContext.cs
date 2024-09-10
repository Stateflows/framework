namespace Stateflows.Activities.Context.Interfaces
{
    public interface IEventContext<out TEvent> : IActivityActionContext
    {
        TEvent Event { get; }
    }
}
