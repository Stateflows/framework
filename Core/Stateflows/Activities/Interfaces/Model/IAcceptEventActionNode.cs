using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IAcceptEventActionNode<in TEvent> : IActivityNode
    {
        Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
    }

    public interface IAcceptEventActionNode : IAcceptEventActionNode<object>, IAbstractAction
    {
        Task IAcceptEventActionNode<object>.ExecuteAsync(object @event, CancellationToken cancellationToken)
            => ExecuteAsync(cancellationToken);
    }
}
