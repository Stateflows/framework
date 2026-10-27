namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Describes an available HTTP operation associated with a behavior response.</summary>
public struct HateoasLink
{
    /// <summary>Gets or sets the link's relation, such as an event or custom operation.</summary>
    public string Rel { get; set; }
    /// <summary>Gets or sets the operation's URI or route template.</summary>
    public string Href { get; set; }
    /// <summary>Gets or sets the accepted HTTP method or comma-separated methods.</summary>
    public string Method { get; set; }
}