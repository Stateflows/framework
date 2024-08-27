using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityExceptionHandler
    {
        Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => Task.FromResult(false);

        Task<bool> OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.FromResult(false);

    }
}
