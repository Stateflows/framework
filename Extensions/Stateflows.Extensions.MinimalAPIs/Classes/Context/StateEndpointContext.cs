using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IStateContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IStateContext;

namespace Stateflows.Extensions.MinimalAPIs;

public class StateEndpointContext : StateMachineEndpointContext, IStateEndpointContext
{
    public IStateContext State { get; internal set; }
}