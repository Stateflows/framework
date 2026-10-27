using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.StateMachines;

/// <summary>Defines custom HTTP endpoints alongside a typed State Machine definition.</summary>
public interface IStateMachineEndpoints : IStateMachine
{
    /// <summary>Maps the State Machine class's custom endpoints during API mapping.</summary>
    /// <param name="endpointsBuilder">Builder for routes relative to the State Machine instance route.</param>
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}