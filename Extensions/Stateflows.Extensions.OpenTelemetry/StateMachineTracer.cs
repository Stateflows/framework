using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class StateMachineTracer(ILogger<StateMachineTracer> logger)
        : IStateMachineObserver, IStateMachineInterceptor, IStateMachineExceptionHandler
    {
        private bool Skip = false;
        
        internal static readonly ActivitySource Source = new(nameof(Stateflows));

        private Activity? EventProcessingActivity;
        public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        {
            try
            {
                var noTracing =
                    context.Event!.GetType().GetCustomAttributes<NoTracingAttribute>().Any() ||
                    context.Headers.Values.Any(h => h is NoTracing);
                
                Skip = noTracing;
                
                if (!noTracing)
                {
                    var eventName = context.Event is Completion
                        ? "Completion"
                        : context.Event.GetType().GetEventName().ToShortName();
                    
                    var header = context.Headers.Values.FirstOrDefault(h => h is ActivityHeader);
                    if (header is ActivityHeader activityHeader)
                    {
                        EventProcessingActivity = Source.StartActivity(
                            $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing {eventName}",
                            ActivityKind.Internal,
                            parentContext: activityHeader.ActivityContext
                        );
                    }
                    
                    EventProcessingActivity ??= Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing {eventName}",
                        ActivityKind.Internal,
                        default(ActivityContext),
                        [
                            new KeyValuePair<string, object?>("StateMachineId", $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}"),
                            new KeyValuePair<string, object?>("Event", eventName)
                        ]
                    );
                    
                    logger.LogTrace(
                        message: "State Machine '{StateMachineId}' received event '{Event}', processing",
                        $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                        eventName
                    );
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeProcessEvent), e);
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            try
            {
                if (context.Behavior.Id.Type != BehaviorType.StateMachine)
                {
                    return;
                }
                
                EventProcessingActivity?.AddTag("EventStatus", eventStatus);
                
                if (!Skip)
                {
                    if (eventStatus != EventStatus.Failed)
                    {
                        StopProcessingActivity(context, eventStatus);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(AfterProcessEvent), e);
            }
        }
        
        private Activity? GuardActivity;
        private Activity? DefaultGuardActivity;
        public void BeforeTransitionGuard<TEvent>(ITransitionContext<TEvent> context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                var eventName = context.Event!.GetType().GetEventName().ToShortName();
                var triggerEventName = context.TriggerType.GetEventName().ToShortName();
                if (eventName != Event<Completion>.Name.ToShortName())
                {
                    if (DefaultGuardActivity != null)
                    {
                        DefaultGuardActivity.Stop();
                        DefaultGuardActivity = null;
                    }
                    
                    if (GuardActivity != null)
                    {
                        GuardActivity.Stop();
                        GuardActivity = null;
                    }

                    GuardActivity = Source.StartActivity(
                        triggerEventName == eventName
                            ? context.Target == null
                                ? $"Internal transition ({context.Source.Name.ToShortName()})/{eventName}"
                                : $"Transition ({context.Source.Name.ToShortName()}) –{eventName}🡢 ({context.Target?.Name.ToShortName()})"
                            : context.Target == null
                                ? $"Internal transition ({context.Source.Name.ToShortName()})/{triggerEventName} triggered by {eventName}"
                                : $"Transition ({context.Source.Name.ToShortName()}) –{triggerEventName}🡢 ({context.Target?.Name.ToShortName()}) triggered by {eventName}",
                        ActivityKind.Internal,
                        StateEntryActivity?.Context ?? EventProcessingActivity?.Context ?? default(ActivityContext)
                    );

                    if (context.ExecutionTrigger != (object)context.Event && GuardActivity != null)
                    {
                        GuardActivity.DisplayName = GuardActivity.DisplayName.Replace(eventName, $"deferred {eventName}");
                    }
                }
                else
                {
                    if (DefaultGuardActivity != null)
                    {
                        DefaultGuardActivity.Stop();
                        DefaultGuardActivity = null;
                    }
                    
                    if (GuardActivity != null)
                    {
                        GuardActivity.Stop();
                        GuardActivity = null;
                    }
                    
                    DefaultGuardActivity = Source.StartActivity(
                        context.Target == null
                            ? $"Default transition ({context.Source.Name.ToShortName()})"
                            : $"Default transition ({context.Source.Name.ToShortName()}) 🡢 ({context.Target?.Name.ToShortName()})",
                        ActivityKind.Internal,
                        StateEntryActivity?.Context ?? EventProcessingActivity?.Context ?? default(ActivityContext)
                    );
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeTransitionGuard), e);
            }
        }

        public void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                var eventName = context.Event!.GetType().GetEventName().ToShortName();
                if (eventName != Event<Completion>.Name.ToShortName())
                {
                    if (guardResult || GuardActivity == null) return;
                    
                    GuardActivity.Stop();
                    GuardActivity.DisplayName += " blocked by guard";
                }
                else
                {
                    if (guardResult || DefaultGuardActivity == null) return;
                    
                    DefaultGuardActivity.Stop();
                    DefaultGuardActivity.DisplayName += " blocked by guard";
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(AfterTransitionGuard), e);
            }
        }
        
        private Activity? EffectActivity;
        public void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                var parentContext = DefaultGuardActivity?.Context ??
                    GuardActivity?.Context ??
                    EventProcessingActivity?.Context ??
                    default(ActivityContext);
                
                var eventName = context.Event!.GetType().GetEventName().ToShortName();
                var traceName = context.Target == null
                    ? $"Internal transition effect"
                    : eventName == Event<Completion>.Name.ToShortName()
                        ? $"Default transition effect"
                        : $"Transition effect";
                
                EffectActivity = parentContext != null
                    ? Source.StartActivity(
                        traceName,
                        ActivityKind.Internal,
                        (ActivityContext)parentContext
                    )
                    : Source.StartActivity(traceName);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeTransitionEffect), e);
            }
        }

        public void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                EffectActivity?.Stop();
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(AfterTransitionEffect), e);
            }
        }

        private Activity? InitializerActivity;
        private bool ImplicitInitialization;
        public void BeforeStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization)
        {
            try
            {
                ImplicitInitialization = implicitInitialization;
                if (Skip && !ImplicitInitialization) return;
                
                if (EventProcessingActivity == null)
                {
                    var header = context.Headers.Values.FirstOrDefault(h => h is ActivityHeader);
                    if (header is ActivityHeader activityHeader)
                    {
                        InitializerActivity = Source.StartActivity(
                            $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' initialized{(ImplicitInitialization ? " implicitly" : "")}",
                            ActivityKind.Internal,
                            parentContext: activityHeader.ActivityContext
                        );
                    }
                    
                    InitializerActivity ??= Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' initialized{(ImplicitInitialization ? " implicitly" : "")}"
                    );
                    
                    EventProcessingActivity = InitializerActivity;
                }
                else
                {
                    InitializerActivity = Source.StartActivity($"State Machine initialized{(ImplicitInitialization ? " implicitly" : "")}");
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeStateMachineInitialize), e);
            }
        }

        public void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;
                
                if (InitializerActivity != null)
                {
                    if (!initialized)
                    {
                        InitializerActivity.DisplayName =
                            $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' not initialized";
                    }

                    InitializerActivity.Stop();
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(AfterStateMachineInitialize), e);
            }
        }
        
        private Activity? FinalizerActivity;
        public void BeforeStateMachineFinalize(IStateMachineActionContext context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;
                
                FinalizerActivity = Source.StartActivity("State Machine finalized");
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeStateMachineFinalize), e);
            }
        }

        public void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;
                
                FinalizerActivity?.Stop();
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(AfterStateMachineFinalize), e);
            }
        }

        public void BeforeStateInitialize(IStateActionContext context)
        { }

        public void AfterStateInitialize(IStateActionContext context)
        { }

        public void BeforeStateFinalize(IStateActionContext context)
        { }

        public void AfterStateFinalize(IStateActionContext context)
        { }

        private Activity? StateEntryActivity;
        public void BeforeStateEntry(IStateActionContext context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                var parentContext = DefaultGuardActivity?.Context ??
                    GuardActivity?.Context ??
                    EventProcessingActivity?.Context ??
                    default(ActivityContext);
                
                if (parentContext != null)
                {
                    StateEntryActivity = Source.StartActivity(
                        $"State ({context.State.Name.ToShortName()})/entry",
                        ActivityKind.Internal,
                        (ActivityContext)parentContext
                    );
                }
                else
                {
                    StateEntryActivity = Source.StartActivity($"State ({context.State.Name.ToShortName()})/entry");
                }
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeStateEntry), e);
            }
        }

        public void AfterStateEntry(IStateActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            StateEntryActivity?.Stop();
        }
        
        private Activity? StateExitActivity;
        public void BeforeStateExit(IStateActionContext context)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return;

                var parentContext = DefaultGuardActivity?.Context ??
                    GuardActivity?.Context ?? 
                    EventProcessingActivity?.Context ??
                    default(ActivityContext);

                var traceName = $"State ({context.State.Name.ToShortName()})/exit";
                StateExitActivity = parentContext != null
                    ? Source.StartActivity(
                        traceName,
                        ActivityKind.Internal,
                        (ActivityContext)parentContext
                    )
                    : Source.StartActivity(traceName);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(BeforeStateExit), e);
            }
        }

        public void AfterStateExit(IStateActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            StateExitActivity?.Stop();
        }

        public bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (InitializerActivity != null)
                {
                    InitializerActivity.Stop();
                    InitializerActivity.SetStatus(ActivityStatusCode.Error);
                    InitializerActivity.AddException(exception);
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineInitializationException), e);
            }

            return false;
        }

        public bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (FinalizerActivity != null)
                {
                    FinalizerActivity.Stop();
                    FinalizerActivity.SetStatus(ActivityStatusCode.Error);
                    FinalizerActivity.AddException(exception);
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineFinalizationException), e);
            }

            return false;
        }

        public bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (GuardActivity != null)
                {
                    GuardActivity.Stop();
                    GuardActivity.SetStatus(ActivityStatusCode.Error);
                    GuardActivity.AddException(exception);
                    GuardActivity = null;
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionGuardException), e);
            }

            return false;
        }

        public bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (EffectActivity != null)
                {
                    EffectActivity.Stop();
                    EffectActivity.SetStatus(ActivityStatusCode.Error);
                    EffectActivity.AddException(exception);
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionEffectException), e);
            }

            return false;
        }

        public bool OnStateInitializationException(IStateActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            return false;
        }

        public bool OnStateFinalizationException(IStateActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            return false;
        }

        public bool OnStateEntryException(IStateActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (StateEntryActivity != null)
                {
                    StateEntryActivity.Stop();
                    StateEntryActivity.SetStatus(ActivityStatusCode.Error);
                    StateEntryActivity.AddException(exception);
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateEntryException), e);
            }

            return false;
        }

        public bool OnStateExitException(IStateActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return false;

                if (StateExitActivity != null)
                {
                    StateExitActivity.Stop();
                    StateExitActivity.SetStatus(ActivityStatusCode.Error);
                    StateExitActivity.AddException(exception);
                }

                StopProcessingActivity(context, EventStatus.Failed, exception);
            }
            catch (Exception e)
            {
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateExitException), e);
            }
            
            return false;
        }

        private void StopProcessingActivity(IBehaviorActionContext context, EventStatus eventStatus, Exception exception = null!)
        {
            var eventName = context.ExecutionTrigger is Completion
                ? "Completion"
                : context.ExecutionTrigger.GetType().GetEventName().ToShortName();
            
            if (exception == null!)
            {
                logger.LogTrace(
                    message: "State Machine '{StateMachineId}' processed event '{Event}' with result '{EventStatus}'",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    eventName,
                    eventStatus
                );
            }
            else
            {
                logger.LogError(
                    message: "State Machine '{StateMachineId}' failed to process event '{Event}'",
                    exception: exception,
                    args:
                    [
                        $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                        eventName
                    ]
                );

                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
                EventProcessingActivity?.AddException(exception);
            }

            if (DefaultGuardActivity != null)
            {
                DefaultGuardActivity.Stop();
                DefaultGuardActivity = null;
            }
         
            if (GuardActivity != null)
            {
                GuardActivity.Stop();
                GuardActivity = null;
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