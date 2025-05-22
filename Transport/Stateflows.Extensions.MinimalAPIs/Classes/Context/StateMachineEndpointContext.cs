using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

public class StateMachineEndpointContext : BehaviorEndpointContext, IStateMachineEndpointContext
{
    public IStateMachineContext StateMachine { get; internal set; }
}