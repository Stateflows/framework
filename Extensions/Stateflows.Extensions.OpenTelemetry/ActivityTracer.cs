using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TracingActivity = System.Diagnostics.Activity;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class ActivityTracer : IActivityObserver, IActivityInterceptor, IActivityExceptionHandler
    {
        private readonly ILogger<ActivityTracer> Logger;
        public ActivityTracer(ILogger<ActivityTracer> logger)
        {
            Logger = logger;
        }

        private bool Skip = false;
        
        private TracingActivity? EventProcessingActivity;
        public bool BeforeProcessEvent<TEvent>(Activities.Context.Interfaces.IEventContext<TEvent> context)
        {
            var noTracing =
                context.Event!.GetType().GetCustomAttributes<NoTracingAttribute>().Any() ||
                context.Headers.Any(h => h is NoTracing);
            
            if (noTracing)
            {
                Skip = true;
            }
            else
            {
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    EventProcessingActivity = StateMachineTracer.Source.StartActivity(
                        $"Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetShortEventName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                else
                {
                    EventProcessingActivity = StateMachineTracer.Source.StartActivity(
                        $"Activity '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetShortEventName()}'"
                    );
                }

                Logger.LogTrace(
                    message: "Activity '{ActivityId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                    context.Event.GetType().GetShortEventName()
                );
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(Activities.Context.Interfaces.IEventContext<TEvent> context, EventStatus eventStatus)
        {
            lock (ExecutionActivities)
            {
                ExecutionActivities.Clear();
            }

            lock (FlowActivationActivities)
            {
                FlowActivationActivities.Clear();
                NodeIncomingFlowsActivities.Clear();
            }

            if (!Skip)
            {
                if (eventStatus != EventStatus.Failed)
                {
                    StopProcessingActivity(context, eventStatus);
                }
            }
        }

        private void StopProcessingActivity(IBehaviorActionContext context, EventStatus eventStatus, Exception exception = null!)
        {
            if (exception == null!)
            {
                Logger.LogTrace(
                    message: "Activity '{ActivityId}' processed event '{Event}' with result '{EventStatus}'",
                    $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                    context.ExecutionTrigger.GetType().GetShortEventName(),
                    eventStatus
                );
            }
            else
            {
                Logger.LogError(
                    message: "Activity '{ActivityId}' failed to process event '{Event}'",
                    exception: exception,
                    args: new object[]
                    {
                        $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                        context.ExecutionTrigger.GetType().GetShortEventName()
                    }
                );

                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
            }

            // if (DefaultGuardActivity != null)
            // {
            //     DefaultGuardActivity.Stop();
            //     DefaultGuardActivity = null;
            // }
            //
            // if (GuardActivity != null)
            // {
            //     GuardActivity.Stop();
            //     GuardActivity = null;
            // }

            if (EventProcessingActivity != null)
            {
                EventProcessingActivity.Stop();
                EventProcessingActivity.DisplayName += $": {eventStatus.ToString()}";
                EventProcessingActivity = null;
            }
        }
        
        public void BeforeActivityInitialize(IActivityInitializationContext context)
        { }

        public void AfterActivityInitialize(IActivityInitializationContext context, bool initialized)
        { }

        public void BeforeActivityFinalize(IActivityFinalizationContext context)
        { }

        public void AfterActivityFinalize(IActivityFinalizationContext context)
        { }

        private readonly Dictionary<string, TracingActivity> InitializationActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeInitialize(IActivityNodeContext context)
        {
            var activity = StateMachineTracer.Source.StartActivity($"Node '{context.CurrentNode.Name}'");
            if (activity == null) return;
            lock (InitializationActivities) InitializationActivities.Add(context.CurrentNode.Name, activity);
        }

        public void AfterNodeInitialize(IActivityNodeContext context)
        {
            lock (InitializationActivities)
                if (InitializationActivities.TryGetValue(context.CurrentNode.Name, out var activity))
                {
                    activity.DisplayName += " initialized";
                    activity.Stop();
                    InitializationActivities.Remove(context.CurrentNode.Name);
                }
        }

        private readonly Dictionary<string, TracingActivity> FinalizationActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeFinalize(IActivityNodeContext context)
        {
            var activity = StateMachineTracer.Source.StartActivity($"Node '{context.CurrentNode.Name}'");
            if (activity == null) return;
            lock (FinalizationActivities) FinalizationActivities.Add(context.CurrentNode.Name, activity);
        }

        public void AfterNodeFinalize(IActivityNodeContext context)
        {
            lock (FinalizationActivities)
                if (FinalizationActivities.TryGetValue(context.CurrentNode.Name, out var activity))
                {
                    activity.DisplayName += " finalized";
                    activity.Stop();
                    FinalizationActivities.Remove(context.CurrentNode.Name);
                }
        }

        private readonly Dictionary<string, TracingActivity> ActivationActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeActivate(IActivityNodeContext context, bool activated)
        {
            TracingActivity? parentActivity = null;
            lock (FlowActivationActivities)
            {
                if (NodeIncomingFlowsActivities.TryGetValue(context.CurrentNode.Name, out var flowActivities))
                {
                    parentActivity = flowActivities.Last();
                }
            }

            var activity = parentActivity != null
                ? StateMachineTracer.Source.StartActivity(
                    $"Node '{context.CurrentNode.Name}'",
                    ActivityKind.Internal,
                    parentActivity.Context
                )
                : StateMachineTracer.Source.StartActivity($"Node '{context.CurrentNode.Name}'");
            
            if (activity == null) return;
            lock (ActivationActivities) ActivationActivities.Add(context.CurrentNode.Name, activity);
        }

        public void AfterNodeActivate(IActivityNodeContext context)
        {
            lock (ActivationActivities)
                if (ActivationActivities.TryGetValue(context.CurrentNode.Name, out var activity))
                {
                    activity.DisplayName += " activated";
                    activity.Stop();
                    ActivationActivities.Remove(context.CurrentNode.Name);
                }
        }

        private readonly Dictionary<string, TracingActivity> ExecutionActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeExecute(IActivityNodeContext context)
        {
            var activity = StateMachineTracer.Source.StartActivity($"Node '{context.CurrentNode.Name}'");
            if (activity == null) return;
            lock (ExecutionActivities) ExecutionActivities.Add(context.CurrentNode.Name, activity);
        }

        public void AfterNodeExecute(IActivityNodeContext context)
        {
            lock (ExecutionActivities)
                if (ExecutionActivities.TryGetValue(context.CurrentNode.Name, out var activity))
                {
                    activity.DisplayName += " executed";
                    activity.Stop();
                }
        }

        private readonly Dictionary<string, TracingActivity> FlowActivationActivities = new Dictionary<string, TracingActivity>();
        private readonly Dictionary<string, List<TracingActivity>> NodeIncomingFlowsActivities = new Dictionary<string, List<TracingActivity>>();
        public void BeforeFlowActivate(IActivityFlowContext context)
        {
            TracingActivity parentActivity;
            lock (ExecutionActivities)
            {
                ExecutionActivities.TryGetValue(context.SourceNode.Name, out parentActivity);
            }

            var tokenName = context.TokenType.GetTokenName();
            var flowIdentifier = $"{context.SourceNode.Name}-{tokenName}->{context.TargetNode.Name}";
            
            var activity = StateMachineTracer.Source.StartActivity(
                context.TokenType == typeof(ControlToken)
                    ? $"Control flow ⦗{context.SourceNode.Name}⦘ 🡢 ⦗{context.TargetNode.Name}⦘"
                    : $"Flow ⦗{context.SourceNode.Name}⦘ –{tokenName}🡢 ⦗{context.TargetNode.Name}⦘",
                ActivityKind.Internal,
                parentActivity!.Context
            );
            
            if (activity == null) return;

            lock (FlowActivationActivities)
            {
                FlowActivationActivities.Add(flowIdentifier, activity);
            }
        }

        public void AfterFlowActivate(IActivityFlowContext context, bool activated)
        {
            var tokenName = context.TokenType.GetTokenName();
            var flowIdentifier = $"{context.SourceNode.Name}-{tokenName}->{context.TargetNode.Name}";
            lock (FlowActivationActivities)
                if (FlowActivationActivities.TryGetValue(flowIdentifier, out var activity))
                {
                    activity.Stop();
                    if (!activated)
                    {
                        activity.DisplayName += ": not activated";
                    }
                    FlowActivationActivities.Remove(flowIdentifier);
                
                    if (!NodeIncomingFlowsActivities.TryGetValue(context.TargetNode.Name, out var flows))
                    {
                        flows = new List<TracingActivity>();
                        NodeIncomingFlowsActivities.Add(context.TargetNode.Name, flows);
                    }
                
                    flows.Add(activity);
                }
        }

        public void BeforeFlowGuard<TToken>(IGuardContext<TToken> context)
        { }

        public void AfterFlowGuard<TToken>(IGuardContext<TToken> context, bool guardResult)
        { }

        public void BeforeFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken> context)
        { }

        public void AfterFlowTransform<TToken, TTransformedToken>(ITransformationContext<TToken, TTransformedToken> context)
        { }

        public void AfterHydrate(IActivityActionContext context)
        { }

        public void BeforeDehydrate(IActivityActionContext context)
        { }

        public bool OnActivityInitializationException(IActivityInitializationContext context, Exception exception)
            => false;

        public bool OnActivityFinalizationException(IActivityFinalizationContext context, Exception exception)
            => false;

        public bool OnNodeInitializationException(IActivityNodeContext context, Exception exception)
            => false;

        public bool OnNodeFinalizationException(IActivityNodeContext context, Exception exception)
            => false;

        public bool OnNodeExecutionException(IActivityNodeContext context, Exception exception)
            => false;

        public bool OnFlowGuardException<TToken>(IGuardContext<TToken> context, Exception exception)
            => false;

        public bool OnFlowTransformationException<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
            => false;
    }
}