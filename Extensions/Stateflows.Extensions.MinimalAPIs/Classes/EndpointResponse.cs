namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Contains behavior links and metadata returned by a custom endpoint.</summary>
/// <param name="links">Available operations for the current behavior configuration.</param>
/// <param name="metadata">Behavior metadata exposed to the caller.</param>
public class EndpointResponse(IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
{
    /// <summary>Gets the available behavior operation links.</summary>
    public IEnumerable<HateoasLink> Links { get; } = links;
        
    /// <summary>Determines whether the links collection is nonempty for serializer conventions.</summary>
    /// <returns>True when at least one link is present.</returns>
    public bool ShouldSerializeLinks()
        => Links.Any();
        
    /// <summary>Gets the behavior metadata exposed to the caller.</summary>
    public IDictionary<string, object> Metadata { get; } = metadata;

    /// <summary>Determines whether the metadata dictionary is nonempty for serializer conventions.</summary>
    /// <returns>True when at least one metadata entry is present.</returns>
    public bool ShouldSerializeMetadata()
        => Metadata.Any();
}

/// <summary>Adds an application result to a custom endpoint's behavior response envelope.</summary>
/// <typeparam name="TResult">Application result type.</typeparam>
/// <param name="result">Application result.</param>
/// <param name="links">Available operations for the current behavior configuration.</param>
/// <param name="metadata">Behavior metadata exposed to the caller.</param>
public class EndpointResponse<TResult>(TResult result, IEnumerable<HateoasLink> links, IDictionary<string, object> metadata)
    : EndpointResponse(links, metadata)
{
    /// <summary>Gets the application result.</summary>
    public TResult Result { get; } = result;
    
    /// <summary>Evaluates the result's default-value condition used by serializer conventions.</summary>
    /// <returns>True when the result equals the default value of <typeparamref name="TResult"/>.</returns>
    public bool ShouldSerializeResult()
        => EqualityComparer<TResult>.Default.Equals(Result, default);
}