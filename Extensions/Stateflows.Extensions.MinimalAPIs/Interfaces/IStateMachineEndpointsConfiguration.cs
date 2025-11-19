using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

public interface IStateMachineEndpointsConfiguration : IStateMachine
{
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}