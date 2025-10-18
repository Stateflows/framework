using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

public interface IStateMachineEndpoints : IStateMachine
{
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}