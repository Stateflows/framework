using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
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
        public bool BeforeProcessEvent<TEvent>(StateMachines.Context.Interfaces.IEventContext<TEvent> context)
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
                    EventProcessingActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetEventName().GetShortName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                else
                {
                    EventProcessingActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetEventName().GetShortName()}'"
                    );
                }

                Logger.LogTrace(
                    message: "State Machine '{StateMachineId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                    context.Event.GetType().GetEventName().GetShortName()
                );
            }
            
            return true;
        }

        public void AfterProcessEvent<TEvent>(StateMachines.Context.Interfaces.IEventContext<TEvent> context, EventStatus eventStatus)
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

            var eventName = context.Event!.GetType().GetEventName().GetShortName();
            if (eventName != Event<Completion>.Name.GetShortName())
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
                    context.Target == null
                        ? $"Internal transition ({context.Source.Name.GetShortName()})/{eventName}"
                        : $"Transition ({context.Source.Name.GetShortName()}) –{eventName}🡢 ({context.Target?.Name.GetShortName()})"
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
                
                DefaultGuardActivity = Source.StartActivity($"Default transition ({context.Source.Name.GetShortName()}) 🡢 ({context.Target?.Name.GetShortName()})");
            }
        }

        public void AfterTransitionGuard<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            if (Skip && !ImplicitInitialization) return;

            var eventName = context.Event!.GetType().GetEventName().GetShortName();
            if (eventName != Event<Completion>.Name.GetShortName())
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

            var eventName = context.Event!.GetType().GetEventName().GetShortName();
            EffectActivity = Source.StartActivity(
                context.Target == null
                    ? $"Internal transition effect"
                    : eventName == Event<Completion>.Name.GetShortName()
                        ? $"Default transition effect" 
                        : $"Transition effect",
                ActivityKind.Internal,
                parentContext: DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity.Context
            );
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
            
            if (EventProcessingActivity == null && Skip)
            {
                
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    InitializerActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' initialized implicitly",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                else
                {
                    InitializerActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}' initialized implicitly"
                    );
                }
                
                EventProcessingActivity = InitializerActivity;
            }
            else
            {
                InitializerActivity = Source.StartActivity($"State machine initialized{(ImplicitInitialization ? " implicitly" : "")}");
            }
        }

        public void AfterStateMachineInitialize(IStateMachineInitializationContext context, bool initialized)
        {
            if (Skip && !ImplicitInitialization) return;
            
            InitializerActivity?.Stop();
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

            StateEntryActivity = Source.StartActivity(
                $"State ({context.CurrentState.Name.GetShortName()})/entry",
                ActivityKind.Internal,
                parentContext: DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity.Context
            );
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
            
            StateExitActivity = Source.StartActivity(
                $"State ({context.CurrentState.Name.GetShortName()})/exit",
                ActivityKind.Internal,
                parentContext: DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity.Context
            );
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
                InitializerActivity.AddException(exception);
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
                FinalizerActivity.AddException(exception);
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
                GuardActivity.AddException(exception);
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
                EffectActivity.AddException(exception);
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
                StateEntryActivity.AddException(exception);
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
                StateExitActivity.AddException(exception);
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
                    $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                    context.ExecutionTrigger.GetType().GetEventName().GetShortName(),
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
                        $"{context.Behavior.Id.Name.GetShortName()}:{context.Behavior.Id.Instance}",
                        context.ExecutionTrigger.GetType().GetEventName().GetShortName()
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