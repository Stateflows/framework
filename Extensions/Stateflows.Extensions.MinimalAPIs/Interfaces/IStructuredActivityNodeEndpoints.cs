using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

/// <summary>Defines custom HTTP endpoints available while a structured Activity node is active.</summary>
public interface IStructuredActivityNodeEndpoints : IStructuredActivityNode
{
    /// <summary>Maps node-scoped custom endpoints during API mapping.</summary>
    /// <param name="endpointsBuilder">Builder for routes filtered by the node's active configuration.</param>
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}