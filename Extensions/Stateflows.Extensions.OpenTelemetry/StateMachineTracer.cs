using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry
{
    public class StateMachineTracer : IStateMachineObserver, IStateMachineInterceptor, IStateMachineExceptionHandler
    {
        private readonly ILogger<StateMachineTracer> Logger;
        public StateMachineTracer(ILogger<StateMachineTracer> logger)
        {
            Logger = logger;
        }

        private bool Skip = false;
        
        internal static readonly ActivitySource Source = new ActivitySource(nameof(Stateflows));

        private Activity? EventProcessingActivity;
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
                    EventProcessingActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                
                EventProcessingActivity ??= Source.StartActivity(
                    $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' processing '{context.Event.GetType().GetEventName().ToShortName()}'"
                );

                Logger.LogTrace(
                    message: "State Machine '{StateMachineId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.Event.GetType().GetEventName().ToShortName()
                );
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
        {
            if (context.Behavior.Id.Type != BehaviorType.StateMachine)
            {
                return;
            }
            
            if (!Skip)
            {
                if (eventStatus != EventStatus.Failed)
                {
                    StopProcessingActivity(context, eventStatus);
                }
            }
        }
        
        private Activity? GuardActivity;
        private Activity? DefaultGuardActivity;
        public void BeforeTransitionGuard<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip && !ImplicitInitialization) return;

            var eventName = context.Event!.GetType().GetEventName().ToShortName();
            var triggerEventName = context.Event!.GetType().GetEventName().ToShortName();
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
                            : $"Transition ({context.Source.Name.ToShortName()}) –{triggerEventName}🡢 ({context.Target?.Name.ToShortName()}) triggered by {eventName}"
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
                
                DefaultGuardActivity = Source.StartActivity($"Default transition ({context.Source.Name.ToShortName()}) 🡢 ({context.Target?.Name.ToShortName()})");
            }
        }

        public void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
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
        
        private Activity? EffectActivity;
        public void BeforeTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip && !ImplicitInitialization) return;

            var parentContext = DefaultGuardActivity?.Context ??
                                GuardActivity?.Context ?? EventProcessingActivity?.Context;
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

        public void AfterTransitionEffect<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip && !ImplicitInitialization) return;

            EffectActivity?.Stop();
        }

        private Activity? InitializerActivity;
        private bool ImplicitInitialization;
        public void BeforeStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization)
        {
            ImplicitInitialization = implicitInitialization;
            if (Skip && !ImplicitInitialization) return;
            
            if (EventProcessingActivity == null)
            {
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    InitializerActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}' initialized{(ImplicitInitialization ? " implicitly" : "")}",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
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

        public void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
        {
            if (Skip && !ImplicitInitialization) return;
            
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
        public void BeforeStateMachineFinalize(IStateMachineActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            FinalizerActivity = Source.StartActivity("State machine finalized");
        }

        public void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            FinalizerActivity?.Stop();
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
            if (Skip && !ImplicitInitialization) return;

            var parentContext = DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity?.Context;
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

        public void AfterStateEntry(IStateActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            StateEntryActivity?.Stop();
        }
        
        private Activity? StateExitActivity;
        public void BeforeStateExit(IStateActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;

            var parentContext = DefaultGuardActivity?.Context ??
                                GuardActivity?.Context ?? 
                                EventProcessingActivity?.Context;

            var traceName = $"State ({context.State.Name.ToShortName()})/exit";
            StateExitActivity = parentContext != null
                ? Source.StartActivity(
                    traceName,
                    ActivityKind.Internal,
                    (ActivityContext)parentContext
                )
                : Source.StartActivity(traceName);
        }

        public void AfterStateExit(IStateActionContext context)
        {
            if (Skip && !ImplicitInitialization) return;
            
            StateExitActivity?.Stop();
        }

        public bool OnStateMachineInitializationException(IStateMachineInitializationContext context, Exception exception)
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

        public bool OnStateMachineFinalizationException(IStateMachineActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            if (FinalizerActivity != null)
            {
                FinalizerActivity.Stop();
                FinalizerActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                FinalizerActivity.SetCustomProperty(nameof(Exception), exception);
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

            return false;
        }

        public bool OnTransitionGuardException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            if (GuardActivity != null)
            {
                GuardActivity.Stop();
                GuardActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                GuardActivity.SetCustomProperty(nameof(Exception), exception);
                GuardActivity = null;
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

            return false;
        }

        public bool OnTransitionEffectException<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            if (EffectActivity != null)
            {
                EffectActivity.Stop();
                EffectActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                EffectActivity.SetCustomProperty(nameof(Exception), exception);
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

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
            if (Skip && !ImplicitInitialization) return false;

            if (StateEntryActivity != null)
            {
                StateEntryActivity.Stop();
                StateEntryActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                StateEntryActivity.SetCustomProperty(nameof(Exception), exception);
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);

            return false;
        }

        public bool OnStateExitException(IStateActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return false;

            if (StateExitActivity != null)
            {
                StateExitActivity.Stop();
                StateExitActivity.SetStatus(ActivityStatusCode.Error);
                // TODO: change to AddException after upgrade
                StateExitActivity.SetCustomProperty(nameof(Exception), exception);
            }

            StopProcessingActivity(context, EventStatus.Failed, exception);

            return false;
        }

        private void StopProcessingActivity(IBehaviorActionContext context, EventStatus eventStatus, Exception exception = null!)
        {
            if (exception == null!)
            {
                Logger.LogTrace(
                    message: "State Machine '{StateMachineId}' processed event '{Event}' with result '{EventStatus}'",
                    $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                    context.ExecutionTrigger.GetType().GetEventName().ToShortName(),
                    eventStatus
                );
            }
            else
            {
                Logger.LogError(
                    message: "State Machine '{StateMachineId}' failed to process event '{Event}'",
                    exception: exception,
                    args: new object[]
                    {
                        $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}",
                        context.ExecutionTrigger.GetType().GetEventName().ToShortName()
                    }
                );

                EventProcessingActivity?.SetStatus(ActivityStatusCode.Error);
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

        public void AfterHydrate(IStateMachineActionContext context)
        { }

        public void BeforeDehydrate(IStateMachineActionContext context)
        { }
    }
}