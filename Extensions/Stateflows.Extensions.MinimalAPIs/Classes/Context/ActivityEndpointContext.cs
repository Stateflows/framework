using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using Stateflows.Activities;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActivityEndpointContext : BehaviorEndpointContext, IActivityEndpointContext
{
    private IReadOnlyTree<IActivityNodeContext>? activeNodes;
    public IReadOnlyTree<IActivityNodeContext> ActiveNodes =>
        activeNodes ??= Activity.ActiveNodes.Translate<IActivityNodeContext>(n => new ActivityNodeContext() { Name = n.Name, NodeType = n.NodeType }).AsReadOnly();
    
    public IActivityContext Activity { get; internal set; }
}