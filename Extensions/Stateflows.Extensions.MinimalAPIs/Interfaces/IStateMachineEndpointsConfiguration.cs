using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

/// <summary>Supplies class-specific HTTP mapping rules from a typed State Machine definition.</summary>
public interface IStateMachineEndpointsConfiguration : IStateMachine
{
    /// <summary>Configures the State Machine's generated and custom endpoints during API mapping.</summary>
    /// <param name="configuration">Configuration scoped to this State Machine class.</param>
    static abstract void ConfigureEndpoints(IBehaviorClassEndpointsConfiguration configuration);
}