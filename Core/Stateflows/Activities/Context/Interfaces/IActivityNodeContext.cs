using System.Threading;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IActivityNodeContext : IActivityActionContext
    {
        INodeContext CurrentNode { get; }

        CancellationToken CancellationToken { get; }
    }
}
