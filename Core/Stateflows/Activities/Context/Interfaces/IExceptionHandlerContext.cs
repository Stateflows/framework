using System;

namespace Stateflows.Activities.Context.Interfaces
{
    public interface IExceptionHandlerContext<out TException> : IActivityNodeContext, IOutput
        where TException : Exception
    {
        TException Exception { get; }

        INodeContext NodeOfOrigin { get; }
    }
}
