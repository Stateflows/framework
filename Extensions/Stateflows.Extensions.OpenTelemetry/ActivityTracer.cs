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
        public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
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
                        $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                
                EventProcessingActivity ??= StateMachineTracer.Source.StartActivity(
                    $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'"
                );

                Logger.LogTrace(
                    message: "Activity '{ActivityId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.Event.GetType().GetEventName().ToShortName()
                );
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
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
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.ExecutionTrigger.GetType().GetEventName().ToShortName(),
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
                        $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                        context.ExecutionTrigger.GetType().GetEventName().ToShortName()
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
        
        private Activity? InitializerActivity;
        private bool ImplicitInitialization;
        public void BeforeActivityInitialize(IActivityInitializationContext context, bool implicitInitialization)
        {
            ImplicitInitialization = implicitInitialization;
            if (Skip && !ImplicitInitialization) return;
            
            if (EventProcessingActivity == null)
            {
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    InitializerActivity = StateMachineTracer.Source.StartActivity(
                        $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' initialized{(ImplicitInitialization ? " implicitly" : "")}",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                
                InitializerActivity ??= StateMachineTracer.Source.StartActivity(
                    $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' initialized{(ImplicitInitialization ? " implicitly" : "")}"
                );
                
                EventProcessingActivity = InitializerActivity;
            }
            else
            {
                var traceName = $"Activity initialized{(ImplicitInitialization ? " implicitly" : "")}";
                InitializerActivity = StateMachineTracer.Source.StartActivity(traceName);
            }
        }

        public void AfterActivityInitialize(IActivityInitializationContext context, bool implicitInitialization, bool initialized)
        {
            if (Skip && !implicitInitialization) return;

            if (InitializerActivity != null)
            {
                if (!initialized)
                {
                    InitializerActivity.DisplayName =
                        $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' not initialized";
                }

                InitializerActivity.Stop();
            }
        }

        private Activity? FinalizerActivity;
        public void BeforeActivityFinalize(IActivityFinalizationContext context)
        {
            if (Skip) return;

            if (EventProcessingActivity != null)
            {
                FinalizerActivity = StateMachineTracer.Source.StartActivity(
                    $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' finalized",
                    ActivityKind.Internal,
                    EventProcessingActivity.Context
                );
            }

            FinalizerActivity ??= StateMachineTracer.Source.StartActivity(
                $"Activity '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' finalized"
            );
        }

        public void AfterActivityFinalize(IActivityFinalizationContext context)
        {
            if (Skip) return;

            if (FinalizerActivity != null)
            {
                FinalizerActivity.Stop();
                FinalizerActivity = null;
            }
        }
        // public void BeforeActivityFinalize(IActivityFinalizationContext context)
        // { }
        //
        // public void AfterActivityFinalize(IActivityFinalizationContext context)
        // { }

        private readonly Dictionary<string, TracingActivity> InitializationActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeInitialize(IActivityNodeContext context)
        {
            var activity = StateMachineTracer.Source.StartActivity($"Node ({context.Node.Name.ToShortName()})");
            if (activity == null) return;
            lock (InitializationActivities)
            {
                InitializationActivities.TryAdd(context.Node.Name, activity);
            }
        }

        public void AfterNodeInitialize(IActivityNodeContext context)
        {
            lock (InitializationActivities)
            {
                if (InitializationActivities.TryGetValue(context.Node.Name, out var activity))
                {
                    activity.DisplayName += " initialized";
                    activity.Stop();
                    InitializationActivities.Remove(context.Node.Name);
                }
            }
        }

        private readonly Dictionary<string, TracingActivity> FinalizationActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeFinalize(IActivityNodeContext context)
        {
            TracingActivity? parentActivity = null;
            lock (ExecutionActivities)
            {
                ExecutionActivities.TryGetValue(context.Node.Name, out parentActivity);
            }

            var traceName = $"Node ({context.Node.Name.ToShortName()})";
            var activity = parentActivity == null
                ? StateMachineTracer.Source.StartActivity(traceName)
                : StateMachineTracer.Source.StartActivity(
                    traceName,
                    ActivityKind.Internal,
                    parentActivity.Context
                );
            
            if (activity == null) return;
            lock (FinalizationActivities)
            {
                FinalizationActivities.TryAdd(context.Node.Name, activity);
            }
        }

        public void AfterNodeFinalize(IActivityNodeContext context)
        {
            lock (FinalizationActivities)
                if (FinalizationActivities.TryGetValue(context.Node.Name, out var activity))
                {
                    activity.DisplayName += " finalized";
                    activity.Stop();
                    FinalizationActivities.Remove(context.Node.Name);
                }
        }

        private readonly Dictionary<string, TracingActivity> ActivationActivities = new Dictionary<string, TracingActivity>();
        private readonly List<string> ActivatedEventAcceptNodes = new List<string>();
        public void BeforeNodeActivate(IActivityNodeContext context, bool activated)
        {
            if (!activated)
            {
                TracingActivity? parentActivity = null;
                if (context.Node.TryGetCurrentFlow(out var flow))
                {
                    var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                    lock (FlowActivationActivities)
                    {
                        FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
                    }
                }

                var traceName = $"Node ({context.Node.Name.ToShortName()})";
                var flowCount = context.Node.IncomingFlows.Count();
                var activatedFlowCount = context.Node.IncomingFlows.Count(f => f.Activated);
                traceName += $" omitted: {activatedFlowCount} of {flowCount} flows activated";
            
                var activity = parentActivity != null
                    ? StateMachineTracer.Source.StartActivity(
                        traceName,
                        ActivityKind.Internal,
                        parentActivity.Context
                    )
                    : StateMachineTracer.Source.StartActivity(traceName);
            
                if (activity == null) return;

                lock (ActivationActivities)
                {
                    ActivationActivities.TryAdd(context.Node.Name, activity);
                }

                return;
            }
            
            if (
                context.Node.NodeType == NodeType.AcceptEventAction ||
                context.Node.NodeType == NodeType.TimeEventAction
            )
            {
                TracingActivity? parentActivity = null;
                if (context.Node.IncomingFlows.Any() &&
                    context.Node.TryGetCurrentFlow(out var flow) && 
                    flow.TargetNode.Name == context.Node.Name
                )
                {
                    var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                    lock (FlowActivationActivities)
                    {
                        FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
                    }
                }
                
                if (parentActivity == null && context.Node.TryGetParentNode(out var parentNode))
                {
                    lock (InitializationActivities)
                    {
                        InitializationActivities.TryGetValue(parentNode.Name, out parentActivity);
                    }
                }
                
                // lock (ActivatedEventAcceptNodes)
                // {
                //     if (ActivatedEventAcceptNodes.Contains(context.Node.Name))
                //     {
                        var traceName =
                            $"Node ({context.Node.Name.ToShortName()}) activated, awaiting for incoming event";
            
                        var activity = parentActivity != null
                            ? StateMachineTracer.Source.StartActivity(
                                traceName,
                                ActivityKind.Internal,
                                parentActivity.Context
                            )
                            : StateMachineTracer.Source.StartActivity(traceName);
            
                        if (activity == null) return;

                        lock (ActivationActivities)
                        {
                            ActivationActivities.TryAdd(context.Node.Name, activity);
                        }
            
                        // activity?.Stop();
            
                        // ActivatedEventAcceptNodes.Remove(context.Node.Name);
                    }
                // }
                
                // lock (ActivatedEventAcceptNodes)
                // {
                //     ActivatedEventAcceptNodes.Add(context.Node.Name);
                // }
            // }
        }

        public void AfterNodeActivate(IActivityNodeContext context)
        {
            // TracingActivity? parentActivity = null;
            // if (context.Node.TryGetCurrentFlow(out var flow) && flow.SourceNode.Name == context.Node.Name)
            // {
            //     var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
            //     lock (FlowActivationActivities)
            //     {
            //         FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
            //     }
            // }
            
            // if (
            //     context.Node.NodeType == NodeType.AcceptEventAction ||
            //     context.Node.NodeType == NodeType.TimeEventAction
            // )
            // {
            //     if (parentActivity == null && context.Node.TryGetParentNode(out var parentNode))
            //     {
            //         lock (InitializationActivities)
            //         {
            //             InitializationActivities.TryGetValue(parentNode.Name, out parentActivity);
            //         }
            //     }
            //     
            //     lock (ActivatedEventAcceptNodes)
            //     {
            //         if (ActivatedEventAcceptNodes.Contains(context.Node.Name))
            //         {
            //             var traceName =
            //                 $"Node ({context.Node.Name.ToShortName()}) activated, awaiting for incoming event";
            //
            //             var activity = parentActivity != null
            //                 ? StateMachineTracer.Source.StartActivity(
            //                     traceName,
            //                     ActivityKind.Internal,
            //                     parentActivity.Context
            //                 )
            //                 : StateMachineTracer.Source.StartActivity(traceName);
            //
            //             activity?.Stop();
            //
            //             ActivatedEventAcceptNodes.Remove(context.Node.Name);
            //         }
            //     }
            // }

            lock (ActivationActivities)
            {
                if (ActivationActivities.TryGetValue(context.Node.Name, out var activity))
                {
                    activity.Stop();
                    ActivationActivities.Remove(context.Node.Name);
                }
            }
        }

        private readonly Dictionary<string, TracingActivity> ExecutionActivities = new Dictionary<string, TracingActivity>();
        public void BeforeNodeExecute(IActivityNodeContext context)
        {
            if (context.Node.NodeType == NodeType.AcceptEventAction)
            {
                lock (ActivatedEventAcceptNodes)
                {
                    if (ActivatedEventAcceptNodes.Contains(context.Node.Name))
                    {
                        ActivatedEventAcceptNodes.Remove(context.Node.Name);
                    }
                }
            }

            TracingActivity? parentActivity = null;
            if (context.Node.TryGetCurrentFlow(out var flow))
            {
                var flowIdentifier = $"{flow.SourceNode.Name}-{flow.TokenType.GetTokenName()}->{flow.TargetNode.Name}";
                lock (FlowActivationActivities)
                {
                    FlowActivationActivities.TryGetValue(flowIdentifier, out parentActivity);
                }
            }

            if (parentActivity == null &&
                (
                    context.Node.NodeType == NodeType.Initial ||
                    context.Node.NodeType == NodeType.Input
                ) &&
                context.Node.TryGetParentNode(out var parentNode)
               )
            {
                lock (InitializationActivities)
                {
                    InitializationActivities.TryGetValue(parentNode.Name, out parentActivity);
                }
            }

            if (parentActivity == null &&
                (
                    context.Node.NodeType == NodeType.AcceptEventAction ||
                    context.Node.NodeType == NodeType.TimeEventAction
                ) &&
                context.Node.TryGetParentNode(out parentNode)
               )
            {
                lock (ExecutionActivities)
                {
                    ExecutionActivities.TryGetValue(parentNode.Name, out parentActivity);
                }
            }

            var traceName = $"Node ({context.Node.Name.ToShortName()})";
            
            var activity = parentActivity != null
                ? StateMachineTracer.Source.StartActivity(
                    traceName,
                    ActivityKind.Internal,
                    parentActivity.Context
                )
                : StateMachineTracer.Source.StartActivity(traceName);
            
            if (activity == null) return;
            lock (ExecutionActivities)
            {
                ExecutionActivities.TryAdd(context.Node.Name, activity);
            }
        }

        public void AfterNodeExecute(IActivityNodeContext context)
        {
            lock (ExecutionActivities)
            {
                if (ExecutionActivities.TryGetValue(context.Node.Name, out var activity))
                {
                    if (!activity.DisplayName.EndsWith(" executed"))
                    {
                        activity.DisplayName += " executed";
                    }

                    activity.Stop();
                }
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
                tokenName = tokenName.ToShortName();
                var targetTokenName = context.TargetTokenType.GetTokenName().ToShortName();

                var activity = StateMachineTracer.Source.StartActivity(
                    context.TokenType == typeof(ControlToken)
                        ? $"Control flow ({context.SourceNode.Name.ToShortName()}) 🡢 ({context.TargetNode.Name.ToShortName()})"
                        : tokenName == targetTokenName
                            ? $"Flow ({context.SourceNode.Name.ToShortName()}) –{tokenName} ({context.TokenCount})🡢 ({context.TargetNode.Name.ToShortName()})"
                            : $"Flow ({context.SourceNode.Name.ToShortName()}) –{tokenName}/{targetTokenName} ({context.TokenCount})🡢 ({context.TargetNode.Name.ToShortName()})",
                    ActivityKind.Internal,
                    parentActivity!.Context
                );

                if (activity == null) return;

                lock (FlowActivationActivities)
                {
                    FlowActivationActivities.TryAdd(flowIdentifier, activity);
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
        {
            if (Skip && !ImplicitInitialization) return false;

            if (InitializerActivity != null)
            {
                InitializerActivity.Stop();
                InitializerActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                InitializerActivity.SetCustomProperty(nameof(Exception), exception);
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

            return false;
        }

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