using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IExceptionHandlerNode<in TException> : IActivityNode
        where TException : Exception
    {
        Task HandleAsync(TException exception, CancellationToken cancellationToken);
    }
}
