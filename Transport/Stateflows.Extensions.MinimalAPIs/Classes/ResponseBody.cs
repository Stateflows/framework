using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

internal class ResponseBody(SendResult result, IEnumerable<EventHolder> notifications, IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
{
    public EventStatus Status { get; } = result.Status;
        
    public string StatusText { get; } = Enum.GetName(typeof(EventStatus), result.Status)!;

    public EventValidation EventValidation { get; } = result.Validation;
    
    public bool ShouldSerializeEventValidation()
        => !EventValidation.IsValid;
    
    public IEnumerable<EventHolder> Notifications { get; } = notifications;
    
    public bool ShouldSerializeNotifications()
        => Notifications.Any();

    public IEnumerable<HateoasLink> Links { get; } = links;
        
        public bool ShouldSerializeLinks()
            => Links.Any();
        
    public IDictionary<string, object> Metadata { get; } = metadata;

    public bool ShouldSerializeMetadata()
        => Metadata.Any();
}

internal class ResponseBody<TResponse>(RequestResult<TResponse> result, IEnumerable<EventHolder> notifications, IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
    : ResponseBody(result, notifications, links, metadata)
{
    public TResponse Response { get; } = result.Response;
    
    public bool ShouldSerializeResponse()
        => Response != null;
}