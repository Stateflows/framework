using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public sealed class AcceptEventActionNode<TEvent> : IAcceptEventActionNode<TEvent>
        where TEvent : Event, new()
    {
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public static string Name => ActivityNode<AcceptEventActionNode<TEvent>>.Name;
    }
}
