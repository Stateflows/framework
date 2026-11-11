using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Runtime model element representing an action node that accepts an event and produces output tokens.</summary>
    /// <typeparam name="TEvent">The event type accepted by the node.</typeparam>
    public sealed class AcceptAndOutputEventActionNode<TEvent>(IOutputTokens<TEvent> outputTokens)
        : IAcceptEventActionNode<TEvent>, ITokenProducer<TEvent>
    {
        /// <inheritdoc />
        public Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken)
        {
            outputTokens.Add(@event);
            
            return Task.CompletedTask;
        }

        /// <summary>Gets the registered model name of this node type.</summary>
        public static string Name => ActivityNode<AcceptAndOutputEventActionNode<TEvent>>.Name;
    }
}
