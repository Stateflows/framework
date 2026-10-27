using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

/// <summary>Defines custom HTTP endpoints alongside a typed Activity definition.</summary>
public interface IActivityEndpoints : IActivity
{
    /// <summary>Maps the Activity class's custom endpoints during API mapping.</summary>
    /// <param name="endpointsBuilder">Builder for routes relative to the Activity instance route.</param>
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}