using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

public interface IPublishes<TReturn>
{
    /// <summary>
    /// Publishes a notification to all subscribers and watchers of current behavior
    /// </summary>
    /// <typeparam name="TNotification">Type of notification</typeparam>
    /// <param name="notification">Notification event instance</param>
    /// <param name="headers">Notification event headers</param>
    void Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
        => PublishRange([ notification ], headers);
    
    void PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader> headers = null);

    /// <summary>
    /// Publishes timed notification to all subscribers and watchers of current behavior
    /// </summary>
    /// <typeparam name="TNotification">Type of notification</typeparam>
    /// <param name="notification">Notification event instance</param>
    /// <param name="timeToLiveInSeconds">Notification time-to-live in seconds (default value: 60 seconds)</param>
    /// <param name="headers">Notification event headers</param>
    void PublishTimed<TNotification>(TNotification notification, int timeToLiveInSeconds = 60, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(TimeToLive), new TimeToLive(timeToLiveInSeconds) } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        Publish(notification, headersList);
    }
    
    void PublishRangeTimed<TNotification>(IEnumerable<TNotification> notifications, int timeToLiveInSeconds = 60, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(TimeToLive), new TimeToLive(timeToLiveInSeconds) } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        PublishRange(notifications, headersList);
    }

    /// <summary>
    /// Publishes retained notification to all subscribers and watchers of current behavior
    /// </summary>
    /// <typeparam name="TNotification">Type of notification</typeparam>
    /// <param name="notification">Notification event instance</param>
    /// <param name="headers">Notification event headers</param>
    void PublishRetained<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(Retain), new Retain() } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        Publish(notification, headersList);
    }
    
    void PublishRangeRetained<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(Retain), new Retain() } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        PublishRange(notifications, headersList);
    }
}