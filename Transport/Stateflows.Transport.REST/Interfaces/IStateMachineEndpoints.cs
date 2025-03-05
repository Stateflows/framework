using Stateflows.Transport.REST;

namespace Stateflows.StateMachines;

public interface IStateMachineEndpoints : IStateMachine
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}