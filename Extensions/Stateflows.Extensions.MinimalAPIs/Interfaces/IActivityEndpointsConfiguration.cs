using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IActivityEndpointsConfiguration : IActivity
{
    void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}