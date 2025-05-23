namespace Stateflows.Extensions.MinimalAPIs;

internal class RequestBody
{
    public string[]? RequestedNotifications { get; set; } = [];
}

internal class RequestBody<TEvent> : RequestBody
{
    public required TEvent Event { get; set; }
}