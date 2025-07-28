using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

public interface IStateMachineEndpointsConfiguration : IStateMachine
{
    void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}