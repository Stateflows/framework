using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

public interface IActionEndpoints : IAction
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}