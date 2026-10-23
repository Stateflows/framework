using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>
    /// Acknowledges activity failures without recovering execution. All handlers are awaited sequentially;
    /// returning true acknowledges the exception. Execution still fails and its context
    /// changes are not persisted. IDE break-on-thrown settings control debugger stops
    /// independently of acknowledgement. Graph exception-handler nodes are separate.
    /// </summary>
    public interface IActivityExceptionHandler
    {
        Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception);

        Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception);

        Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception);

        Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception);

    }
}
