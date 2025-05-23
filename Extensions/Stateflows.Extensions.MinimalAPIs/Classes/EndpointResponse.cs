namespace Stateflows.Extensions.MinimalAPIs;

public class EndpointResponse(IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
{
    public IEnumerable<HateoasLink> Links { get; } = links;
        
    public bool ShouldSerializeLinks()
        => Links.Any();
        
    public IDictionary<string, object> Metadata { get; } = metadata;

    public bool ShouldSerializeMetadata()
        => Metadata.Any();
}

public class EndpointResponse<TResult>(TResult result, IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
    : EndpointResponse(links, metadata)
{
    public TResult Result { get; } = result;
    
    public bool ShouldSerializeResult()
        => EqualityComparer<TResult>.Default.Equals(Result, default);
}