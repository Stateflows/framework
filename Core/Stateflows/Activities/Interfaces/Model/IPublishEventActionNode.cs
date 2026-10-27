namespace Stateflows.Activities
{
    /// <summary>Defines an event-producing node that publishes its generated event.</summary>
    /// <typeparam name="TEvent">The event type published by the node.</typeparam>
    public interface IPublishEventActionNode<TEvent> : IEventActionNode<TEvent>;
}
