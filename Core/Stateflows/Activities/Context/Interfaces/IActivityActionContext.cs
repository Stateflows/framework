using Stateflows.Common;

namespace Stateflows.Activities;

public interface IActivityActionContext : IBehaviorActionContext
{
    /// <summary>
    /// Behavior-level lock handle enabling developer to synchronize operations between Activity's nodes
    /// </summary>
    object LockHandle { get; }

    /// <summary>
    /// Tree of Nodes that represents current configuration of Activity behavior instance
    /// </summary>
    IReadOnlyTree<INodeContext> ActiveNodes { get; }// => Activity.ActiveNodes;
}
