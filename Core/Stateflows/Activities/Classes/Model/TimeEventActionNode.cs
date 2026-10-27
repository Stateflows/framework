using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing an action node that accepts a time event.</summary>
    /// <typeparam name="TEvent">The time-event type accepted by the node.</typeparam>
    public sealed class TimeEventActionNode<TEvent> : IAcceptEventActionNode<TEvent>
        where TEvent : TimeEvent, new()
    {
        /// <inheritdoc />
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<TimeEventActionNode<TEvent>>.Name;
    }
}
