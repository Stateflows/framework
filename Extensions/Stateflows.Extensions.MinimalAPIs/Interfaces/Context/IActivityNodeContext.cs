using Stateflows.Activities;

namespace Stateflows.Extensions.MinimalAPIs.Interfaces;

public interface IActivityNodeContext
{
    public string Name { get; }

    /// <summary>
    /// Type of the node
    /// </summary>
    NodeType NodeType { get; }
}