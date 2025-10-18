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
        public override void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            var executor = context.Behavior.GetExecutor();
            if (!executor.StateHasChanged) return;
            var notification = new ActivityInfo()
            {
                Id = executor.Context.Id,
                BehaviorStatus = executor.BehaviorStatus,
                ActiveNodes = executor.GetNodesTree(),
                ExpectedEvents = executor.GetExpectedEventNamesAsync().GetAwaiter().GetResult(),
            };

            context.Behavior.Publish(notification);
            context.Behavior.Publish<BehaviorInfo>(notification);
            
            foreach (var flowIdentifier in executor.Context.ActivatedFlows)
            {
                executor.Context.FlowTokensCount.Remove(flowIdentifier);
            }
        }

        public override bool OnNodeExecutionException(IActivityNodeContext context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on execution of node {context.Node.Name}");

            return false;
        }

        public override bool OnActivityInitializationException(IActivityInitializationContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on Activity initialization");

            return false;
        }

        public override bool OnActivityFinalizationException(IActivityFinalizationContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on Activity finalization");

            return false;
        }

        public override bool OnNodeInitializationException(IActivityNodeContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on initialization of node {context.Node.Name}");

            return false;
        }

        public override bool OnNodeFinalizationException(IActivityNodeContext context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on finalization of node {context.Node.Name}");

            return false;
        }

        public override bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception)
        {            
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on guard of flow from node {context.SourceNode.Name}");

            return false;
        }

        public override bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': unhandled exception '{exception.GetType().Name}' thrown with message '{exception.Message}' on transformation of flow from node {context.SourceNode.Name}");

            return false;
        }

        public override void AfterActivityInitialize(IActivityInitializationContext context, bool implicitInitialization, bool initialized)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': {(initialized ? "" : "not ")}initialized{(implicitInitialization ? " implicitly" : "")}");
        }

        public override void AfterActivityFinalize(IActivityFinalizationContext context)
        {
            Trace.WriteLine($"⦗→s⦘ Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}': finalized");
            
            var stateflowsContext = ((BaseContext)context).Context.Context;
            
            valueStorage.RemoveAsync(stateflowsContext.Id, CommonValues.ForceFinalizeKey).GetAwaiter().GetResult();
        }
    }
}