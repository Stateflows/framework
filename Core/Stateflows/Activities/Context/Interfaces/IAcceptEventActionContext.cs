namespace Stateflows.Activities.Context.Interfaces
{
    /// <summary>Provides an accepted event to an activity node action.</summary>
    /// <typeparam name="TEvent">The event type accepted by the node.</typeparam>
    public interface IAcceptEventActionContext<out TEvent> : IActionContext
    {
        /// <summary>The event instance that activated this action.</summary>
        TEvent Event { get; }
    }
}
