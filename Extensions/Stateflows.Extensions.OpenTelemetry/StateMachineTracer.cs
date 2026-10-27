using System.Threading.Tasks;
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
    /// <summary>Creates diagnostic activities for state-machine event processing, lifecycle hooks, and transitions.</summary>
    /// <param name="logger">The logger used to report state-machine processing outcomes.</param>
    public class StateMachineTracer(ILogger<StateMachineTracer> logger)
        : IStateMachineObserver, IStateMachineInterceptor, IStateMachineExceptionHandler
    {
        private bool Skip = false;

        internal static readonly ActivitySource Source = new(nameof(Stateflows));

        private Activity? EventProcessingActivity;
        /// <inheritdoc/>
        public async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            Before(context);

            var status = EventStatus.Failed;
            try
            {
                return status = await next();
            }
            catch (Exception exception)
            {
                if (!Skip && EventProcessingActivity != null)
                {
                    EventProcessingActivity.SetTag("EventStatus", EventStatus.Failed);
                    StopProcessingActivity(context, EventStatus.Failed, exception);
                }
                throw;
            }
            finally
            {
                After(context, status);
            }

            void Before(IEventContext<TEvent> context)
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
                            : Event.GetName(context.Event).ToShortName();

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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(ProcessEventAsync), e);
                }

            }

            void After(IEventContext<TEvent> context, EventStatus eventStatus)
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(ProcessEventAsync), e);
                }
            }
        }

        private Activity? GuardActivity;
        private Activity? DefaultGuardActivity;
        /// <inheritdoc/>
        public async Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
        {
            Before(context);
            var result = await next();
            After(context, result);

            void Before(ITransitionContext<TEvent> context)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    var eventName = Event.GetName(context.Event!).ToShortName();
                    var triggerEventName = context.Event is StructuralEvent structural
                        ? structural.Name.ToShortName()
                        : context.TriggerType.GetEventName().ToShortName();
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionGuardAsync), e);
                }
            }

            void After(ITransitionContext<TEvent> context, bool guardResult)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    var eventName = Event.GetName(context.Event!).ToShortName();
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionGuardAsync), e);
                }
            }
        }

        private Activity? EffectActivity;
        /// <inheritdoc/>
        public async Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
        {
            Before(context);
            await next();
            After(context);

            void Before(ITransitionContext<TEvent> context)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    var parentContext = DefaultGuardActivity?.Context ??
                        GuardActivity?.Context ??
                        EventProcessingActivity?.Context ??
                        default(ActivityContext);

                    var eventName = Event.GetName(context.Event!).ToShortName();
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionEffectAsync), e);
                }
            }

            void After(ITransitionContext<TEvent> context)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    EffectActivity?.Stop();
                }
                catch (Exception e)
                {
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionEffectAsync), e);
                }
            }
        }

        private Activity? InitializerActivity;
        private bool ImplicitInitialization;
        /// <inheritdoc/>
        public async Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            Before(context, implicitInitialization);
            var result = await next();
            After(context, implicitInitialization, result);

            void Before(IStateMachineInitializationContext context, bool implicitInitialization)
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineInitializeAsync), e);
                }
            }

            void After(IStateMachineInitializationContext context, bool implicitInitialization, bool initialized)
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineInitializeAsync), e);
                }
            }
        }

        private Activity? FinalizerActivity;
        /// <inheritdoc/>
        public async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
        {
            Before(context);
            await next();
            After(context);

            void Before(IStateMachineActionContext context)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    FinalizerActivity = Source.StartActivity("State Machine finalized");
                }
                catch (Exception e)
                {
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineFinalizeAsync), e);
                }
            }

            void After(IStateMachineActionContext context)
            {
                try
                {
                    if (Skip && !ImplicitInitialization) return;

                    FinalizerActivity?.Stop();
                }
                catch (Exception e)
                {
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineFinalizeAsync), e);
                }
            }
        }

        /// <inheritdoc/>
        public Task OnStateInitializeAsync(IStateActionContext context, Func<Task> next)
            => next();

        /// <inheritdoc/>
        public Task OnStateFinalizeAsync(IStateActionContext context, Func<Task> next)
            => next();

        private Activity? StateEntryActivity;
        /// <inheritdoc/>
        public async Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
        {
            Before(context);
            await next();
            After(context);

            void Before(IStateActionContext context)
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateEntryAsync), e);
                }
            }

            void After(IStateActionContext context)
            {
                if (Skip && !ImplicitInitialization) return;

                StateEntryActivity?.Stop();
            }
        }

        private Activity? StateExitActivity;
        /// <inheritdoc/>
        public async Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
        {
            Before(context);
            await next();
            After(context);

            void Before(IStateActionContext context)
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
                    logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateExitAsync), e);
                }
            }

            void After(IStateActionContext context)
            {
                if (Skip && !ImplicitInitialization) return;

                StateExitActivity?.Stop();
            }
        }

        /// <inheritdoc/>
        public Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineInitializationExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateMachineFinalizationExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionGuardExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnTransitionEffectExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
        {
            if (Skip && !ImplicitInitialization) return Task.FromResult(false);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateEntryExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
        {
            try
            {
                if (Skip && !ImplicitInitialization) return Task.FromResult(false);

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
                logger.LogError(LogTemplates.ExceptionLogTemplate, typeof(StateMachineTracer).FullName, nameof(OnStateExitExceptionAsync), e);
            }

            return Task.FromResult(false);
        }

        private void StopProcessingActivity(IBehaviorActionContext context, EventStatus eventStatus, Exception exception = null!)
        {
            var eventName = context.ExecutionTrigger is Completion
                ? "Completion"
                : Event.GetName(context.ExecutionTrigger).ToShortName();

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