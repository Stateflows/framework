using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public interface IBehaviorContext : ISubscriptions, IInjectionScope
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
        void Publish<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null);

        /// <summary>
        /// Publishes timed notification to all subscribers and watchers of current behavior
        /// </summary>
        /// <typeparam name="TNotification">Type of notification</typeparam>
        /// <param name="notification">Notification event instance</param>
        /// <param name="timeToLiveInSeconds">Notification time-to-live in seconds (default value: 60 seconds)</param>
        /// <param name="headers">Notification event headers</param>
        void PublishTimed<TNotification>(TNotification notification, int timeToLiveInSeconds = 60, IEnumerable<EventHeader> headers = null)
        {
            var headersList = new List<EventHeader>() { new TimeToLive(timeToLiveInSeconds) };
            if (headers != null)
            {
                headersList.AddRange(headers);
            }

            Publish(notification, headersList);
        }

        /// <summary>
        /// Publishes retained notification to all subscribers and watchers of current behavior
        /// </summary>
        /// <typeparam name="TNotification">Type of notification</typeparam>
        /// <param name="notification">Notification event instance</param>
        /// <param name="headers">Notification event headers</param>
        void PublishRetained<TNotification>(TNotification notification, IEnumerable<EventHeader> headers = null)
        {
            var headersList = new List<EventHeader>() { new Retain() };
            if (headers != null)
            {
                headersList.AddRange(headers);
            }

            Publish(notification, headersList);
        }
        
        bool IsEmbedded { get; }
    }
}
