namespace Stateflows.Extensions.MinimalAPIs;

public class RequestBody
{
    public string[]? RequestedNotifications { get; set; } = [];
}

public class RequestBody<TEvent> : RequestBody
{
    public required TEvent Event { get; set; }
}