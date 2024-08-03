using System;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IExceptionContext
    {
        INodeContext ProtectedNode { get; }

        INodeContext NodeOfOrigin { get; }
    }

    public interface IExceptionHandlerContext<out TException> : IExceptionContext, IActivityNodeContext, IOutput
        where TException : Exception
    {
        TException Exception { get; }
    }
}
