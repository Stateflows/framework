using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Entities;
using Stateflows.Extensions.OpenTelemetry.Headers;

namespace Stateflows.Extensions.OpenTelemetry
{
    /// <summary>
    /// Traces entity event processing, mutations, computed fields, projections, and failures.
    /// </summary>
    /// <param name="logger">The logger used to report entity processing failures.</param>
    public class EntityTracer(ILogger<EntityTracer> logger) :
        IEntityObserver, IEntityInterceptor, IEntityExceptionHandler
    {
        private readonly AsyncLocal<Activity?> processingActivity = new();

        /// <inheritdoc/>
        public async Task<EventStatus> ProcessEventAsync<TEvent>(
            IEventContext<TEvent> context, Func<Task<EventStatus>> next)
        {
            var previous = processingActivity.Value;
            using var activity = ShouldTrace(context)
                ? StartProcessingActivity(context)
                : null;
            processingActivity.Value = activity;
            try
            {
                var status = await next();
                activity?.SetTag("EventStatus", status);
                if (activity != null)
                {
                    activity.DisplayName += $": {status}";
                    if (status == EventStatus.Failed)
                    {
                        activity.SetStatus(ActivityStatusCode.Error);
                    }
                }

                return status;
            }
            catch (Exception exception)
            {
                if (activity != null)
                {
                    activity.SetTag("EventStatus", EventStatus.Failed);
                    activity.DisplayName += $": {EventStatus.Failed}";
                    RecordException(activity, exception);
                }

                if (ShouldTrace(context))
                {
                    logger.LogError(exception, "Entity '{EntityId}' failed to process event '{Event}'.",
                        GetEntityId(context), Event.GetName(typeof(TEvent)).ToShortName());
                }

                throw;
            }
            finally
            {
                processingActivity.Value = previous;
            }
        }

        /// <inheritdoc/>
        public Task OnMutationAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Func<Task> next)
            => ObserveAsync(context, $"Mutation '{Event.GetName(typeof(TMutationEvent)).ToShortName()}'",
                "Mutation", Event.GetName(typeof(TMutationEvent)), next);

        /// <inheritdoc/>
        public Task OnComputedFieldAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Func<Task<TField>> next)
            => ObserveAsync(context, $"Computed field '{context.FieldName}'", "Field", context.FieldName,
                async () => { await next(); });

        /// <inheritdoc/>
        public Task OnProjectionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Func<Task<TProjection>> next)
            => ObserveAsync(context, $"Projection '{Event.GetName(typeof(TProjection)).ToShortName()}'",
                "Projection", Event.GetName(typeof(TProjection)), async () => { await next(); });

        /// <inheritdoc/>
        public Task<bool> OnEntityInitializationExceptionAsync(
            IEntityInitializationExceptionContext context, Exception exception)
        {
            processingActivity.Value?.SetTag("ImplicitInitialization", context.ImplicitInitialization);
            return HandleExceptionAsync(context, exception, "initialization");
        }

        /// <inheritdoc/>
        public Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(
            IMutationObservationContext<TTemplate, TMutationEvent> context, Exception exception)
            => HandleExceptionAsync(context, exception, $"mutation '{Event.GetName(typeof(TMutationEvent)).ToShortName()}'");

        /// <inheritdoc/>
        public Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(
            IComputedFieldObservationContext<TTemplate> context, Exception exception)
            => HandleExceptionAsync(context, exception, $"computed field '{context.FieldName}'");

        /// <inheritdoc/>
        public Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(
            IProjectionObservationContext<TTemplate> context, Exception exception)
            => HandleExceptionAsync(context, exception, $"projection '{Event.GetName(typeof(TProjection)).ToShortName()}'");

        private async Task ObserveAsync(
            IBehaviorActionContext context, string name, string tagName, string tagValue, Func<Task> next)
        {
            using var activity = processingActivity.Value != null && ShouldTrace(context)
                ? StateMachineTracer.Source.StartActivity(name)
                : null;
            activity?.SetTag("EntityId", GetEntityId(context));
            activity?.SetTag(tagName, tagValue);
            try
            {
                await next();
            }
            catch (Exception exception)
            {
                if (activity != null)
                {
                    RecordException(activity, exception);
                }

                throw;
            }
        }

        private Task<bool> HandleExceptionAsync(IBehaviorActionContext context, Exception exception, string operation)
        {
            var root = processingActivity.Value;
            if (ShouldTrace(context))
            {
                if (root != null)
                {
                    RecordException(root, exception);
                    if (Activity.Current is { } current && current != root &&
                        current.Source == StateMachineTracer.Source &&
                        Equals(current.GetTagItem("EntityId"), root.GetTagItem("EntityId")))
                    {
                        RecordException(current, exception);
                    }
                }

                logger.LogError(exception, "Entity '{EntityId}' failed during {Operation}.", GetEntityId(context), operation);
            }

            return Task.FromResult(false);
        }

        private static Activity? StartProcessingActivity<TEvent>(IEventContext<TEvent> context)
        {
            var eventName = Event.GetName(typeof(TEvent)).ToShortName();
            var name = $"Entity '{GetEntityId(context)}' processing '{eventName}'";
            var parent = context.Headers.Values.OfType<ActivityHeader>()
                .Select(header => header.ActivityContext)
                .FirstOrDefault(value => value != default);
            var activity = parent != default
                ? StateMachineTracer.Source.StartActivity(name, ActivityKind.Internal, parent)
                : StateMachineTracer.Source.StartActivity(name);
            activity?.SetTag("EntityId", GetEntityId(context));
            activity?.SetTag("Event", eventName);
            return activity;
        }

        private static bool ShouldTrace(IBehaviorActionContext context)
            => !context.ExecutionTrigger.GetType().IsDefined(typeof(NoTracingAttribute), inherit: true) &&
                !context.Headers.Values.Any(header => header is NoTracing);

        private static string GetEntityId(IBehaviorActionContext context)
            => $"{context.Behavior.Id.Name.ToShortName()}:{context.Behavior.Id.InstanceText}";

        private static void RecordException(Activity activity, Exception exception)
        {
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
            if (!activity.Events.Any(@event => @event.Name == "exception" &&
                    @event.Tags.Any(tag => tag.Key == "exception.stacktrace" && Equals(tag.Value, exception.ToString()))))
            {
                activity.AddException(exception);
            }
        }
    }
}
