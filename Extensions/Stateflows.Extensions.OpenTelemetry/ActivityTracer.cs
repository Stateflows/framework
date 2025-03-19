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
using Stateflows.Activities.Registration.Interfaces.Base;
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
                        $"Activity '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetEventName().GetShortName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                else
                {
                    EventProcessingActivity = StateMachineTracer.Source.StartActivity(
                        $"Activity '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetEventName().GetShortName()}'"
                    );
                }

                Logger.LogTrace(
                    message: "Activity '{ActivityId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                    context.Event.GetType().GetEventName().GetShortName()
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
                // NodeIncomingFlowsActivities.Clear();
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
                    $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                    context.ExecutionTrigger.GetType().GetEventName().GetShortName(),
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
                        $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                        context.ExecutionTrigger.GetType().GetEventName().GetShortName()
                    }
                );

                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
            }

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
            var activity = StateMachineTracer.Source.StartActivity($"Node ({context.CurrentNode.Name.GetShortName()})");
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
            var activity = StateMachineTracer.Source.StartActivity($"Node ({context.CurrentNode.Name.GetShortName()})");
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
        private readonly List<string> ActivatedEventAcceptNodes = new List<string>();
        public void BeforeNodeActivate(IActivityNodeContext context, bool activated)
        {
            if (!activated)
            {
                TracingActivity? parentActivity = null;
                if (context.CurrentNode.TryGetCurrentFlow(out var flow))
                {
                    var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                    lock (FlowActivationActivities)
                        FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
                }

                var traceName = $"Node ({context.CurrentNode.Name.GetShortName()})";
                var flowCount = context.CurrentNode.IncomingFlows.Count();
                var activatedFlowCount = context.CurrentNode.IncomingFlows.Count(f => f.Activated);
                traceName += $" omitted: {activatedFlowCount} of {flowCount} flows activated";
            
                var activity = parentActivity != null
                    ? StateMachineTracer.Source.StartActivity(
                        traceName,
                        ActivityKind.Internal,
                        parentActivity.Context
                    )
                    : StateMachineTracer.Source.StartActivity(traceName);
            
                if (activity == null) return;
            
                lock (ActivationActivities) ActivationActivities.Add(context.CurrentNode.Name, activity);

                return;
            }
            
            if (context.CurrentNode.NodeType == NodeType.AcceptEventAction)
            {
                lock (ActivatedEventAcceptNodes) ActivatedEventAcceptNodes.Add(context.CurrentNode.Name);
            }
        }

        public void AfterNodeActivate(IActivityNodeContext context)
        {
            TracingActivity? parentActivity = null;
            if (context.CurrentNode.TryGetCurrentFlow(out var flow))
            {
                var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                lock (FlowActivationActivities)
                    FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
            }

            if (context.CurrentNode.NodeType == NodeType.AcceptEventAction)
                lock (ActivatedEventAcceptNodes)
                    if (ActivatedEventAcceptNodes.Contains(context.CurrentNode.Name))
                    {
                        var traceName = $"Node ({context.CurrentNode.Name.GetShortName()}) activated, awaiting for incoming event";

                        var activity = parentActivity != null
                            ? StateMachineTracer.Source.StartActivity(traceName, ActivityKind.Internal, parentActivity.Context)
                            : StateMachineTracer.Source.StartActivity(traceName);

                        activity?.Stop();
                        
                        ActivatedEventAcceptNodes.Remove(context.CurrentNode.Name);
                    }
            
            lock (ActivationActivities)
                if (ActivationActivities.TryGetValue(context.CurrentNode.Name, out var activity))
                {
                    activity.Stop();
                    ActivationActivities.Remove(context.CurrentNode.Name);
                }
        }

        private readonly Dictionary<string, TracingActivity> ExecutionActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeExecute(IActivityNodeContext context)
        {
            if (context.CurrentNode.NodeType == NodeType.AcceptEventAction)
                lock (ActivatedEventAcceptNodes)
                    if (ActivatedEventAcceptNodes.Contains(context.CurrentNode.Name))
                        ActivatedEventAcceptNodes.Remove(context.CurrentNode.Name);
            
            TracingActivity? parentActivity = null;
            if (context.CurrentNode.TryGetCurrentFlow(out var flow))
            {
                var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                lock (FlowActivationActivities)
                    FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
            }

            if (parentActivity == null &&
                (
                    context.CurrentNode.NodeType == NodeType.Initial ||
                    context.CurrentNode.NodeType == NodeType.Input
                ) &&
                context.CurrentNode.TryGetParentNode(out var parentNode)
            )
            {
                lock (InitializationActivities)
                    InitializationActivities.TryGetValue(parentNode.Name, out parentActivity);
            }

            var traceName = $"Node ({context.CurrentNode.Name.GetShortName()})";
            
            var activity = parentActivity != null
                ? StateMachineTracer.Source.StartActivity(
                    traceName,
                    ActivityKind.Internal,
                    parentActivity.Context
                )
                : StateMachineTracer.Source.StartActivity(traceName);
            
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
        public void BeforeFlowActivate(IActivityBeforeFlowContext context)
        {
            TracingActivity parentActivity;
            lock (ExecutionActivities)
            {
                ExecutionActivities.TryGetValue(context.SourceNode.Name, out parentActivity);
            }

            if (parentActivity == null)
            {
                lock (ActivationActivities)
                {
                    ActivationActivities.TryGetValue(context.SourceNode.Name, out parentActivity);
                }
            }

            if (parentActivity != null)
            {
                var tokenName = context.TokenType.GetTokenName();
                var flowIdentifier = $"{context.SourceNode.Name}-{tokenName}->{context.TargetNode.Name}";
                tokenName = tokenName.GetShortName();
                var targetTokenName = context.TargetTokenType.GetTokenName().GetShortName();

                var activity = StateMachineTracer.Source.StartActivity(
                    context.TokenType == typeof(ControlToken)
                        ? $"Control flow ({context.SourceNode.Name.GetShortName()}) 🡢 ({context.TargetNode.Name.GetShortName()})"
                        : tokenName == targetTokenName
                            ? $"Flow ({context.SourceNode.Name.GetShortName()}) –{tokenName} ({context.TokenCount})🡢 ({context.TargetNode.Name.GetShortName()})"
                            : $"Flow ({context.SourceNode.Name.GetShortName()}) –{tokenName}/{targetTokenName} ({context.TokenCount})🡢 ({context.TargetNode.Name.GetShortName()})",
                    ActivityKind.Internal,
                    parentActivity!.Context
                );

                if (activity == null) return;

                lock (FlowActivationActivities)
                {
                    FlowActivationActivities.Add(flowIdentifier, activity);
                }
            }
        }

        public void AfterFlowActivate(IActivityAfterFlowContext context, bool activated)
        {
            var tokenName = context.TokenType.GetTokenName();
            var flowIdentifier = $"{context.SourceNode.Name}-{tokenName}->{context.TargetNode.Name}";
            lock (FlowActivationActivities)
                if (FlowActivationActivities.TryGetValue(flowIdentifier, out var activity))
                {
                    var displayName = activity.DisplayName;
                    activity.Stop();
                    if (!activated)
                    {
                        displayName += ": not activated";
                    }

                    if (context.TokenCount != context.TargetTokenCount)
                    {
                        displayName = displayName.Replace(
                            $"({context.TokenCount})🡢",
                            $"({context.TargetTokenCount} of {context.TokenCount})🡢"
                        );
                    }

                    activity.DisplayName = displayName;

                    // if (!NodeIncomingFlowsActivities.TryGetValue(context.TargetNode.Name, out var flows))
                    // {
                    //     flows = new List<TracingActivity>();
                    //     NodeIncomingFlowsActivities.Add(context.TargetNode.Name, flows);
                    // }

                    // flows.Add(activity);
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