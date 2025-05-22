using Stateflows.Actions;
using Stateflows.Extensions.MinimalAPIs.Interfaces;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActionEndpointContext : BehaviorEndpointContext, IActionEndpointContext
{
    public IActionContext Action { get; internal set; }
}