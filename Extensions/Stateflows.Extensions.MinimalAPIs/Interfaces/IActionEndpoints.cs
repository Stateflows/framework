using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

/// <summary>Defines custom HTTP endpoints alongside a typed Action definition.</summary>
public interface IActionEndpoints : IAction
{
    /// <summary>Maps the Action class's custom endpoints during API mapping.</summary>
    /// <param name="endpointsBuilder">Builder for routes relative to the Action instance route.</param>
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}