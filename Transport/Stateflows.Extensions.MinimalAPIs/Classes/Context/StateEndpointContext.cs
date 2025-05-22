using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.StateMachines;

namespace Stateflows.Extensions.MinimalAPIs;

public class StateEndpointContext : StateMachineEndpointContext, IStateEndpointContext
{
    public IStateContext State { get; internal set; }
}