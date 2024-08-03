using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public sealed class TimeEventActionNode<TEvent> : IAcceptEventActionNode<TEvent>
        where TEvent : TimeEvent, new()
    {
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public static string Name => ActivityNode<TimeEventActionNode<TEvent>>.Name;
    }
}
