using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

public interface IActionEndpointsConfiguration : IAction
{
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}