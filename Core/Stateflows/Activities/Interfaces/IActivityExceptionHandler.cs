using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityExceptionHandler
    {
        Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception);

        Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception);

        Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception);

        Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception);

        Task<bool> OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception);

    }
}
