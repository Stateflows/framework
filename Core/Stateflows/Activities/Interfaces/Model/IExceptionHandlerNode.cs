using System;
using System.Threading;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IExceptionHandlerNode<in TException> : IActivityNode
        where TException : Exception
    {
        Task HandleAsync(TException exception, CancellationToken cancellationToken);
    }

    public interface IExceptionHandlerNode : IExceptionHandlerNode<Exception>, IAbstractAction
    {
        Task IExceptionHandlerNode<Exception>.HandleAsync(Exception exception, CancellationToken cancellationToken)
            => ExecuteAsync(cancellationToken);
    }
}
