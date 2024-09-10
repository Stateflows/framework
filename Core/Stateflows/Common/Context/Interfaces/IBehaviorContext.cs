using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Context.Interfaces
{
    public interface IBehaviorContext : ISubscriptions
    {
        /// <summary>
        /// Represents identifier of current behavior
        /// </summary>
        BehaviorId Id { get; }

        /// <summary>
        /// Provides access to global context values of current behavior
        /// </summary>
        IContextValues Values { get; }

        /// <summary>
        /// Sends an event to current behavior
        /// </summary>
        /// <typeparam name="TEvent">Type of an event</typeparam>
        /// <param name="event">Event instance</param>
        void Send<TEvent>(TEvent @event);

        /// <summary>
        /// Publishes a notification to all subscribers and watchers of current behavior
        /// </summary>
        /// <typeparam name="TNotificationEvent">Type of a notification</typeparam>
        /// <param name="notification">Notification instance</param>
        void Publish<TNotificationEvent>(TNotificationEvent notification);
    }
}
