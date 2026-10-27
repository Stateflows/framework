namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Contains notification-watch preferences shared by generated event request envelopes.</summary>
public class RequestBody
{
    /// <summary>Gets or sets the notification names requested by the HTTP caller.</summary>
    public string[]? RequestedNotifications { get; set; } = [];
}

/// <summary>Wraps a CLR event and notification-watch preferences for a generated HTTP endpoint.</summary>
/// <typeparam name="TEvent">Event payload type.</typeparam>
public class RequestBody<TEvent> : RequestBody
{
    /// <summary>Gets or sets the event payload to deliver to the addressed behavior.</summary>
    public required TEvent Event { get; set; }
}