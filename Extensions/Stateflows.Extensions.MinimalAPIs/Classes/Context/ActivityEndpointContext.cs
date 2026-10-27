using Stateflows.Common;
using Stateflows.Extensions.MinimalAPIs.Interfaces;
using IActivityNodeContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IActivityNodeContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Implements the request-scoped Activity endpoint context populated by the endpoint filter.</summary>
public class ActivityEndpointContext : BehaviorEndpointContext, IActivityEndpointContext
{
    private IReadOnlyTree<IActivityNodeContext>? activeNodes;
    /// <inheritdoc/>
    public IReadOnlyTree<IActivityNodeContext> ActiveNodes =>
        activeNodes ??= (ElementsTree ?? new Tree<string>())
            .Translate<IActivityNodeContext>(name => new ActivityNodeContext()
            {
                Name = name,
                NodeType = ActivityVisitor.GetNodeType(Behavior.Id.Name, name)
            })
            .AsReadOnly();
}