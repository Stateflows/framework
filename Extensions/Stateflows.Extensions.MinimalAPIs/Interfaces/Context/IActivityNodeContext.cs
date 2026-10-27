using Stateflows.Activities;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

/// <summary>Describes an active Activity node exposed to an HTTP handler.</summary>
public interface IActivityNodeContext
{
    /// <summary>Gets the registered node name.</summary>
    public string Name { get; }

    /// <summary>
    /// Type of the node
    /// </summary>
    NodeType NodeType { get; }
}