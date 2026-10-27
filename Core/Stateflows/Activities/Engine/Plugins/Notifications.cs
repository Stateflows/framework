using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Common;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities.Engine
{
    internal class Notifications(IStateflowsValueStorage valueStorage) : ActivityPlugin
    {
        public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            var status = EventStatus.Failed;
            try
            {
                return status = await next();
            }
            finally
            {
                After(context, status);
            }

            void After(IEventContext<TEvent> context, EventStatus eventStatus)
            {
                var executor = context.Behavior.GetExecutor();
                if (!executor.StateHasChanged) return;
                var notification = new ActivityInfo()
                {
                    Id = executor.Context.Id,
                    BehaviorStatus = executor.BehaviorStatus,
                    ActiveNodes = executor.GetNodesTree(),
                    ExpectedEvents = executor.GetExpectedEventNames(),
                };

                context.Behavior.Publish(notification);
                context.Behavior.Publish<BehaviorInfo>(notification);

                foreach (var flowIdentifier in executor.Context.ActivatedFlows)
                {
                    executor.Context.FlowTokensCount.Remove(flowIdentifier);
                }
            }
        }

        public override Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on execution of node {context.Node.Name} - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on Activity initialization - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on Activity finalization - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on initialization of node {context.Node.Name} - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on finalization of node {context.Node.Name} - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on guard of flow from node {context.SourceNode.Name} - {exception}");

            return Task.FromResult(false);
        }

        public override Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' on transformation of flow from node {context.SourceNode.Name} - {exception}");

            return Task.FromResult(false);
        }

        public override async Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            var result = await next();
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': {(result ? "" : "not ")}initialized{(implicitInitialization ? " implicitly" : "")}");
        }

        public override async Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
        {
            await next();
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': finalized");

            var stateflowsContext = ((BaseContext)context).Context.Context;

            context.Behavior.GetExecutor().AddPendingTask(valueStorage.RemoveAsync(stateflowsContext.Id, CommonValues.ForceFinalizeKey));
        }
    }
}