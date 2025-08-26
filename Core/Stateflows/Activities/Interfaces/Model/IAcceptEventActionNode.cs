using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IAcceptEventActionNode<in TEvent> : IActivityNode
    {
        Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
