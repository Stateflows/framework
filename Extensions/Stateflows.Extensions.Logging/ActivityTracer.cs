using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Extensions.Logging
{
    internal class ActivityTracer : IActivityObserver, IActivityInterceptor
    {
        private readonly ILogger logger;

        public ActivityTracer(ILogger<ActivityTracer> logger)
        {
             this.logger = logger;
        }

        public Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
        {
            logger.LogTrace("Activity {id}: behavior is finalized", context.Activity.Id);

            return Task.CompletedTask;
        }

        public Task AfterActivityInitializeAsync(IActivityInitializationContext context)
        {
            logger.LogTrace("Activity {id}: behavior is initialized", context.Activity.Id);

            return Task.CompletedTask;
        }

        public Task AfterFlowGuardAsync<TToken>(IGuardContext<TToken> context, bool guardResult)
        {
            return Task.CompletedTask;
        }

        public Task AfterFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
        {
            return Task.CompletedTask;
        }

        public Task AfterNodeExecuteAsync(IActivityNodeContext context)
        {
            logger.LogTrace("Activity {id}: node {nodeName} is executed", context.Activity.Id, context.CurrentNode.NodeName);

            return Task.CompletedTask;
        }

        public Task AfterNodeFinalizeAsync(IActivityNodeContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterNodeInitializeAsync(IActivityNodeContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeActivityFinalizeAsync(IActivityFinalizationContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeActivityInitializeAsync(IActivityInitializationContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeFlowGuardAsync<TToken>(IGuardContext<TToken> context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeFlowTransformAsync<TToken>(ITransformationContext<TToken> context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeNodeExecuteAsync(IActivityNodeContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeNodeFinalizeAsync(IActivityNodeContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeNodeInitializeAsync(IActivityNodeContext context)
        {
            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IActivityActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task BeforeDehydrateAsync(IActivityActionContext context)
        {
            return Task.CompletedTask;
        }

        public Task<bool> BeforeProcessEventAsync(IEventContext<Event> context)
        {
            return Task.FromResult(true);
        }

        public Task AfterProcessEventAsync(IEventContext<Event> context)
        {
            return Task.CompletedTask;
        }
    }
}
