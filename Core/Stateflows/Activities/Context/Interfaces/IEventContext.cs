namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Context for an activity event handler, including the dispatched event.</summary>
    /// <typeparam name="TEvent">The event type being handled.</typeparam>
    public interface IEventContext<out TEvent> : IActivityActionContext
    {
        /// <summary>The event instance currently being handled.</summary>
        TEvent Event { get; }
    }
}
