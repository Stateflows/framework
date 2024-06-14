using System.Threading;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityNodeContext : IActivityActionContext
    {
        /// <summary>
        /// Information about current node
        /// </summary>
        INodeContext CurrentNode { get; }

        /// <summary>
        /// Cancellation token handling activity execution interruptions, f.e. triggered by FinalNode
        /// </summary>
        CancellationToken CancellationToken { get; }
    }
}
