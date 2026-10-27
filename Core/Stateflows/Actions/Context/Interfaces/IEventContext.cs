namespace Stateflows.Actions.Context.Interfaces
{
    /// <summary>Action delegate context for handling a particular event.</summary>
    /// <typeparam name="TEvent">The event type being handled.</typeparam>
    public interface IEventContext<out TEvent> : IActionDelegateContext
    {
        /// <summary>The event instance being handled.</summary>
        TEvent Event { get; }
    }
}
