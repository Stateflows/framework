using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public sealed class AcceptEventActionNode<TEvent> : IAcceptEventActionNode<TEvent>
    {
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public static string Name => ActivityNode<AcceptEventActionNode<TEvent>>.Name;
    }
}
