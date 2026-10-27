using System.Collections.Generic;

namespace Stateflows.Common.Interfaces;

/// <summary>Publishes notifications to subscribers and active watchers of a behavior.</summary>
/// <typeparam name="TReturn">The context type used by related fluent API contracts.</typeparam>
public interface IPublishes<TReturn>
{
    /// <summary>
    /// Publishes a notification to all subscribers and watchers after the current behavior turn commits.
    /// Use the <see cref="ImmediatePublish"/> header when a notification must be dispatched during execution
    /// and remain visible even if the turn later fails.
    /// </summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notification">The notification payload to publish.</param>
    /// <param name="headers">Optional event headers.</param>
    void Publish<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
        => PublishRange([ notification ], headers);
    
    /// <summary>Publishes multiple notifications after the current behavior turn commits.</summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notifications">The notification payloads to publish.</param>
    /// <param name="headers">Optional headers applied to each notification.</param>
    void PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader> headers = null);

    /// <summary>
    /// Publishes a notification with an explicit time-to-live.
    /// </summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notification">The notification payload to publish.</param>
    /// <param name="timeToLiveInSeconds">The event lifetime in seconds; defaults to 60 seconds.</param>
    /// <param name="headers">Optional additional headers.</param>
    void PublishTimed<TNotification>(TNotification notification, int timeToLiveInSeconds = 60, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(TimeToLive), new TimeToLive(timeToLiveInSeconds) } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        Publish(notification, headersList);
    }
    
    /// <summary>Publishes multiple notifications with an explicit time-to-live.</summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notifications">The notification payloads to publish.</param>
    /// <param name="timeToLiveInSeconds">The lifetime in seconds applied to each notification; defaults to 60 seconds.</param>
    /// <param name="headers">Optional additional headers.</param>
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
    /// Publishes a notification with retention enabled.
    /// </summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notification">The notification payload to publish.</param>
    /// <param name="headers">Optional additional headers.</param>
    void PublishRetained<TNotification>(TNotification notification, IDictionary<string, EventHeader> headers = null)
    {
        var headersList = new Dictionary<string, EventHeader>() { { nameof(Retain), new Retain() } };
        if (headers != null)
        {
            headersList.AddRange(headers);
        }

        Publish(notification, headersList);
    }
    
    /// <summary>Publishes multiple notifications with retention enabled.</summary>
    /// <typeparam name="TNotification">The notification payload type.</typeparam>
    /// <param name="notifications">The notification payloads to publish.</param>
    /// <param name="headers">Optional additional headers applied to each notification.</param>
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