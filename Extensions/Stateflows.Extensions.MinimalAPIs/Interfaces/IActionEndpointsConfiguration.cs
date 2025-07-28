using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

public interface IActionEndpointsConfiguration : IAction
{
    void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}