using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityExceptionHandler
    {
        Task OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
            => Task.CompletedTask;

        Task OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
            => Task.CompletedTask;

        Task OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        Task OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        Task OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
            => Task.CompletedTask;

        Task OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
            => Task.CompletedTask;

        Task OnFlowTransformationExceptionAsync<TToken>(ITransformationContext<TToken> context, Exception exception)
            => Task.CompletedTask;

    }
}
