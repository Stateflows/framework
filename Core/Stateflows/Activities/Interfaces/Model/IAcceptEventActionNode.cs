using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Defines an activity node that executes when an event of the specified type is accepted.</summary>
    /// <typeparam name="TEvent">The event type accepted by the node.</typeparam>
    public interface IAcceptEventActionNode<in TEvent> : IActivityNode
    {
        /// <summary>Executes the node's event action.</summary>
        /// <param name="event">The accepted event.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous execution.</param>
        Task ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
    }

    /// <summary>Defines an untyped accepted-event node implemented as an abstract action.</summary>
    public interface IAcceptEventActionNode : IAcceptEventActionNode<object>, IAbstractAction
    {
        /// <inheritdoc/>
        Task IAcceptEventActionNode<object>.ExecuteAsync(object @event, CancellationToken cancellationToken)
            => ExecuteAsync(cancellationToken);
    }
}
