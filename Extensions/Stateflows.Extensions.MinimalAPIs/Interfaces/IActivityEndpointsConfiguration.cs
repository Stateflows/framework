using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IActivityEndpointsConfiguration : IActivity
{
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}