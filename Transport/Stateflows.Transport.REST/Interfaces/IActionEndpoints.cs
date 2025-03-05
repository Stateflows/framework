using Stateflows.Transport.REST;

namespace Stateflows.Actions;

public interface IActionEndpoints : IAction
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}