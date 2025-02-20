using System.Collections.Generic;
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
        /// <typeparam name="TEvent">Type of event</typeparam>
        /// <param name="event">Event instance</param>
        /// <param name="headers">Event headers</param>
        void Send<TEvent>(TEvent @event, IEnumerable<EventHeader> headers = null);

        /// <summary>
        /// Publishes a notification to all subscribers and watchers of current behavior
        /// </summary>
        /// <typeparam name="TNotification">Type of notification</typeparam>
        /// <param name="notification">Notification event instance</param>
        /// <param name="headers">Notification event headers</param>
        /// <param name="timeToLiveInSeconds">Notification time-to-live (in seconds)</param>
        void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null, int timeToLiveInSeconds = 60);
    }
}
