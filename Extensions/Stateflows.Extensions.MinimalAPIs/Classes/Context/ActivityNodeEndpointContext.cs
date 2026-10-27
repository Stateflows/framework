using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Implements a custom endpoint context scoped to an active structured Activity node.</summary>
public class ActivityNodeEndpointContext : ActivityEndpointContext, IActivityNodeEndpointContext
{
    /// <inheritdoc/>
    public IActivityNodeContext? Node { get; internal set; }
}