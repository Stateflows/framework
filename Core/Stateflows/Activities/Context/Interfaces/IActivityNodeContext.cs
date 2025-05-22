using System.Threading;

namespace Stateflows.Activities
{
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
