using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActivityNodeEndpointContext : ActivityEndpointContext, IActivityNodeEndpointContext
{
    public IActivityNodeContext? Node { get; internal set; }
}