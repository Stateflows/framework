using System.Threading;

namespace Stateflows.Activities
{
    /// <summary>Provides the current activity node, cancellation, and activity-level action context.</summary>
    public interface IActivityNodeContext : IActivityActionContext
    {
        /// <summary>
        /// Information about current node
        /// </summary>
        ICurrentNodeContext Node { get; }

        /// <summary>
        /// Cancellation token handling activity execution interruptions, f.e. triggered by FinalNode
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
