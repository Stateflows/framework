using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

public class ActivityEndpointContext : BehaviorEndpointContext, IActivityEndpointContext
{
    private IReadOnlyTree<IActivityNodeContext>? activeNodes;
    public IReadOnlyTree<IActivityNodeContext> ActiveNodes =>
        activeNodes ??= (ElementsTree ?? new Tree<string>())
            .Translate<IActivityNodeContext>(name => new ActivityNodeContext()
            {
                Name = name,
                NodeType = ActivityVisitor.GetNodeType(Behavior.Id.Name, name)
            })
            .AsReadOnly();
}