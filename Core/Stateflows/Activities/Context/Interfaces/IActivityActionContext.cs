using Stateflows.Common;

namespace Stateflows.Activities;

/// <summary>Provides behavior-level capabilities to an activity action.</summary>
/// <remarks>The context exposes synchronization and a snapshot of the activity's currently active node tree.</remarks>
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
