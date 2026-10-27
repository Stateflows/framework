using Stateflows.Activities;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Describes an active node in an HTTP-visible Activity configuration.</summary>
public class ActivityNodeContext : IActivityNodeContext
{
    /// <inheritdoc/>
    public string Name { get; internal set; }
    /// <inheritdoc/>
    public NodeType NodeType { get; internal set; }
}