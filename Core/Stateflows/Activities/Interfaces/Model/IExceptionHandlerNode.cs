using System;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Defines an activity node that handles an exception of the specified type.</summary>
    /// <typeparam name="TException">The exception type handled by the node.</typeparam>
    public interface IExceptionHandlerNode<in TException> : IActivityNode
        where TException : Exception
    {
        /// <summary>Handles the exception and may emit recovery tokens through the node's action context.</summary>
        /// <param name="exception">The exception being handled.</param>
        /// <param name="cancellationToken">Token used to cancel the asynchronous handling operation.</param>
        Task HandleAsync(TException exception, CancellationToken cancellationToken);
    }

    /// <summary>Defines an exception-handler node for any exception type as an abstract action.</summary>
    public interface IExceptionHandlerNode : IExceptionHandlerNode<Exception>, IAbstractAction
    {
        /// <inheritdoc/>
        Task IExceptionHandlerNode<Exception>.HandleAsync(Exception exception, CancellationToken cancellationToken)
            => ExecuteAsync(cancellationToken);
    }
}
