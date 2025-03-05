using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Extensions.OpenTelemetry.Headers;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;

#nullable enable
namespace Stateflows.Extensions.OpenTelemetry
{
    public class StateMachineTracer : StateMachineObserver, IStateMachineInterceptor, IStateMachineExceptionHandler
    {
        private readonly ILogger<StateMachineTracer> Logger;
        public StateMachineTracer(ILogger<StateMachineTracer> logger)
        {
            Logger = logger;
        }
        
        internal bool Skip = false;
        
        internal static ActivitySource Source = new ActivitySource("Stateflows", "0.17.0-alpha");
        
        internal Activity GuardActivity;
        internal Activity DefaultGuardActivity;
        public override Task BeforeTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip) return Task.CompletedTask;

            var eventName = context.Event.GetType().GetShortEventName();
            if (eventName != Event<Completion>.ShortName)
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
                        ? $"Internal transition ({context.Source.Name})/{eventName}"
                        : $"Transition ({context.Source.Name}) -{eventName}-> ({context.Target?.Name})",
                    ActivityKind.Internal,
                    EventProcessingActivity.Context
                );

                if (context.ExecutionTrigger != (object)context.Event)
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
                    $"Default transition ({context.Source.Name}) -> ({context.Target?.Name})",
                    ActivityKind.Internal,
                    EventProcessingActivity.Context
                );
            }
            
            return Task.CompletedTask;
        }

        public override Task AfterTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, bool guardResult)
        {
            if (Skip) return Task.CompletedTask;

            var eventName = context.Event.GetType().GetShortEventName();
            if (eventName != Event<Completion>.ShortName)
            {
                if (!guardResult)
                {
                    GuardActivity.Stop();
                    GuardActivity.DisplayName += " blocked by guard";    
                }
            }
            else
            {
                if (!guardResult)
                {
                    DefaultGuardActivity.Stop();
                    DefaultGuardActivity.DisplayName += " blocked by guard";    
                }
            }


            return Task.CompletedTask;
        }
        
        internal Activity EffectActivity;
        public override Task BeforeTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip) return Task.CompletedTask;

            var eventName = context.Event.GetType().GetShortEventName();
            EffectActivity = Source.StartActivity(
                // context.Target == null
                //     ? $"Internal transition ({context.Source.Name})/{eventName}: effect"
                //     : eventName == Event<Completion>.ShortName
                //         ? $"Default transition ({context.Source.Name}) -> ({context.Target?.Name}): effect" 
                //         : $"Transition ({context.Source.Name}) -{eventName}-> ({context.Target?.Name}): effect",
                context.Target == null
                    ? $"Internal transition  effect"
                    : eventName == Event<Completion>.ShortName
                        ? $"Default transition  effect" 
                        : $"Transition effect",
                ActivityKind.Internal,
                (ActivityContext)(DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity?.Context)
            );
            
            return Task.CompletedTask;
        }

        public override Task AfterTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context)
        {
            if (Skip) return Task.CompletedTask;

            EffectActivity.Stop();

            return Task.CompletedTask;
        }

        internal Activity StateEntryActivity;
        public override Task BeforeStateEntryAsync(IStateActionContext context)
        {
            if (Skip) return Task.CompletedTask;

            StateEntryActivity = Source.StartActivity(
                $"State ({context.CurrentState.Name})/entry",
                ActivityKind.Internal,
                (ActivityContext)(DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity?.Context)
            );
            
            return Task.CompletedTask;
        }

        public override Task AfterStateEntryAsync(IStateActionContext context)
        {
            if (Skip) return Task.CompletedTask;
            
            StateEntryActivity.Stop();
            StateEntryActivity = null;
            return Task.CompletedTask;
        }
        
        internal Activity StateExitActivity;
        public override Task BeforeStateExitAsync(IStateActionContext context)
        {
            if (Skip) return Task.CompletedTask;
            
            StateExitActivity = Source.StartActivity(
                $"State ({context.CurrentState.Name})/exit",
                ActivityKind.Internal,
                (ActivityContext)(DefaultGuardActivity?.Context ?? GuardActivity?.Context ?? EventProcessingActivity?.Context)
            );
            
            return Task.CompletedTask;
        }

        public override Task AfterStateExitAsync(IStateActionContext context)
        {
            if (Skip) return Task.CompletedTask;
            
            StateExitActivity.Stop();
            StateExitActivity = null;
            return Task.CompletedTask;
        }

        public Task AfterHydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        public Task BeforeDehydrateAsync(IStateMachineActionContext context)
            => Task.CompletedTask;

        internal Activity EventProcessingActivity;
        public Task<bool> BeforeProcessEventAsync<TEvent>(IEventActionContext<TEvent> context)
        {
            return Task.FromResult(true);
            
            // ibis budget reduta bitwy warszawskiej 16
        }

        public Task AfterProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, EventStatus eventStatus)
        {
            return Task.CompletedTask;
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(IEventActionContext<TEvent> context, Func<IEventActionContext<TEvent>, Task<EventStatus>> next)
        {
            var attribute = context.Event.GetType().GetCustomAttribute<NoTracingAttribute>();
            if (attribute != null)
            {
                Skip = true;
            }
            else
            {
                var header = context.Headers.FirstOrDefault(h => h is ActivityHeader);
                if (header is ActivityHeader activityHeader)
                {
                    EventProcessingActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetShortEventName()}'",
                        ActivityKind.Internal,
                        parentContext: activityHeader.Activity.Context
                    );
                }
                else
                {
                    EventProcessingActivity = Source.StartActivity(
                        $"State Machine '{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}' processing '{context.Event.GetType().GetShortEventName()}'"
                    );
                }

                Logger.LogTrace(
                    message: "State Machine '{StateMachineId}' received event '{Event}', processing",
                    $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                    context.Event.GetType().GetShortEventName()
                );
            }

            var eventStatus = await next(context);

            if (!Skip)
            {
                if (eventStatus != EventStatus.Failed)
                {
                    StopProcessingActivity(context, eventStatus);
                }
            }

            return eventStatus;
        }

        public Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        public Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        public Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            GuardActivity.Stop();
            GuardActivity.SetStatus(ActivityStatusCode.Error);
            GuardActivity.AddException(exception);
            GuardActivity = null;
            
            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity.SetStatus(ActivityStatusCode.Error);

            return Task.FromResult(false);
        }

        public Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            EffectActivity.Stop();
            EffectActivity.SetStatus(ActivityStatusCode.Error);
            EffectActivity.AddException(exception);
            
            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity.SetStatus(ActivityStatusCode.Error);

            return Task.FromResult(false);
        }

        public Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        public Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        public Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            StateEntryActivity.Stop();
            StateEntryActivity.SetStatus(ActivityStatusCode.Error);
            StateEntryActivity.AddException(exception);
            
            StopProcessingActivity(context, EventStatus.Failed, exception);
            EventProcessingActivity.SetStatus(ActivityStatusCode.Error);

            return Task.FromResult(false);
        }

        public Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip) return Task.FromResult(false);

            StateExitActivity.Stop();
            StateExitActivity.SetStatus(ActivityStatusCode.Error);
            StateExitActivity.AddException(exception);
            
            StopProcessingActivity(context, EventStatus.Failed, exception);

            return Task.FromResult(false);
        }

        private void StopProcessingActivity(IStateMachineActionContext context, EventStatus eventStatus, Exception exception = null)
        {
            using (Logger.BeginScope(new
                   {
                       EventProcessingActivity.TraceId,
                       ParentId = EventProcessingActivity.ParentSpanId,
                       EventProcessingActivity.SpanId
                   }))
            {
                if (exception == null)
                {
                    Logger.LogTrace(
                        message: "State Machine '{StateMachineId}' processed event '{Event}' with result '{EventStatus}'",
                        $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                        context.ExecutionTrigger.GetType().GetShortEventName(),
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
                            $"{context.Behavior.Id.Name}:{context.Behavior.Id.Instance}",
                            context.ExecutionTrigger.GetType().GetShortEventName()
                        }
                    );

                    EventProcessingActivity.SetStatus(ActivityStatusCode.Error);
                }
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
            
            EventProcessingActivity.Stop();
            EventProcessingActivity.DisplayName += $": {eventStatus.ToString()}";
        }
    }
}