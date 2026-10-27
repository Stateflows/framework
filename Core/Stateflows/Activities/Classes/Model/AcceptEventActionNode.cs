using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing an action node that accepts an event.</summary>
    /// <typeparam name="TEvent">The event type accepted by the node.</typeparam>
    public sealed class AcceptEventActionNode<TEvent> : IAcceptEventActionNode<TEvent>
    {
        /// <inheritdoc />
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<AcceptEventActionNode<TEvent>>.Name;
    }
}
