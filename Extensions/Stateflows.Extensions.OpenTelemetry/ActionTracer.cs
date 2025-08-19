using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TracingActivity = System.Diagnostics.Activity;
using Microsoft.Extensions.Logging;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class ActionTracer : IActionInterceptor, IActionExceptionHandler
    {
        
        private readonly ILogger<ActivityTracer> Logger;
        public ActionTracer(ILogger<ActivityTracer> logger)
        {
            Logger = logger;
        }

        private bool Skip = false;
        
        private TracingActivity? EventProcessingActivity;
        
        public void AfterHydrate(IActionDelegateContext context)
        { }
        
        public void BeforeDehydrate(IActionDelegateContext context)
        { }
        
        public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        {
            var noTracing =
                context.Event!.GetType().GetCustomAttributes<NoTracingAttribute>().Any() ||
                context.Headers.Any(h => h is NoTracing);
            
            Skip = noTracing;
            
            if (!noTracing)
            {
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    EventProcessingActivity = StateMachineTracer.Source.StartActivity(
                        $"Action '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                
                EventProcessingActivity ??= StateMachineTracer.Source.StartActivity(
                    $"Action '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'"
                );

                Logger.LogTrace(
                    message: "Action '{ActionId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.Event.GetType().GetEventName().ToShortName()
                );
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            if (!Skip)
            {
                if (eventStatus != EventStatus.Failed)
                {
                    StopProcessingAction(context, eventStatus);
                }
            }
        }

        private void StopProcessingAction<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus, Exception exception = null!)
        {
            if (exception == null!)
            {
                Logger.LogTrace(
                    message: "Action '{ActionId}' processed event '{Event}' with result '{EventStatus}'",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.ExecutionTrigger.GetType().GetEventName().ToShortName(),
                    eventStatus
                );
            }
            else
            {
                Logger.LogError(
                    message: "Action '{ActionId}' failed to process event '{Event}'",
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

        public bool OnActionException(IActionDelegateContext context, Exception exception)
        {
            if (Skip) return false;

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

            return false;
        }

        private void StopProcessingActivity(IActionDelegateContext context, EventStatus eventStatus, Exception exception = null!)
        {
            if (exception == null!)
            {
                Logger.LogTrace(
                    message: "Action '{ActionId}' processed event '{Event}' with result '{EventStatus}'",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.ExecutionTrigger.GetType().GetEventName().ToShortName(),
                    eventStatus
                );
            }
            else
            {
                Logger.LogError(
                    message: "Action '{ActionId}' failed to process event '{Event}'",
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
    }
}