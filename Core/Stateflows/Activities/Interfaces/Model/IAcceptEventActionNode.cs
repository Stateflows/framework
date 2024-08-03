using Stateflows.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IAcceptEventActionNode<in TEvent> : IActivityNode
        where TEvent : Event, new()
    {
        Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
    }
}
