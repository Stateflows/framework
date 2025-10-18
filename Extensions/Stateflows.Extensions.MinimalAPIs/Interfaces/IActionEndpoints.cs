using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Actions;

public interface IActionEndpoints : IAction
{
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}