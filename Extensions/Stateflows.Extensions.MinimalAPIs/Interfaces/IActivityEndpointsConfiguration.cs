using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

/// <summary>Supplies class-specific HTTP mapping rules from a typed Activity definition.</summary>
public interface IActivityEndpointsConfiguration : IActivity
{
    /// <summary>Configures the Activity's generated and custom endpoints during API mapping.</summary>
    /// <param name="configuration">Configuration scoped to this Activity class.</param>
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}