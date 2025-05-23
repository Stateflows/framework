using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

public interface IStateMachineEndpoints : IStateMachine
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}