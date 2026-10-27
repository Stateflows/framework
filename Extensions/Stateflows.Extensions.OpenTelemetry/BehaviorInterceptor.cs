using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Registration.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

/// <summary>Records behavior event execution counts and durations as metrics.</summary>
/// <param name="meter">The registered meter holding per-resource execution instruments.</param>
public class MetricsInterceptor(StateflowsMeter meter) : BehaviorInterceptor
{
    /// <summary>Runs the downstream behavior pipeline and records its duration and resulting status.</summary>
    /// <typeparam name="TEvent">The event type being processed.</typeparam>
    /// <param name="context">The behavior event context used to select instruments and metric tags.</param>
    /// <param name="next">The remaining event-processing pipeline.</param>
    /// <returns>A task containing the downstream event status.</returns>
    public override async Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var status = EventStatus.Failed;
        try
        {
            return status = await next();
        }
        finally
        {
            Record(status);
        }

        void Record(EventStatus eventStatus)
        {
            stopwatch.Stop();

            var noTracing =
                context.Event!.GetType().GetCustomAttributes<NoTracingAttribute>().Any() ||
                context.Headers.Values.Any(h => h is NoTracing);

            if (noTracing)
            {
                return;
            }

            var eventName = Event.GetName(context.Event);
            var resourceName = meter.ResourcesByBehaviorClass[context.Behavior.Id.BehaviorClass].Name;

            if (meter.ExecutionDurations.TryGetValue(resourceName, out var histogram))
            {
                histogram.Record(
                    stopwatch.Elapsed.TotalMilliseconds,
                    // Behavior instance ids are intentionally not tags: they would make metric cardinality unbounded.
                    new KeyValuePair<string, object?>("behavior.class", context.Behavior.Id.BehaviorClass.ToString()),
                    new KeyValuePair<string, object?>("event.name", eventName),
                    new KeyValuePair<string, object?>("event.status", Enum.GetName(eventStatus))
                );
            }

            if (meter.ExecutionCounters.TryGetValue(resourceName, out var counter))
            {
                counter.Add(
                    1,
                    // Behavior instance ids are intentionally not tags: they would make metric cardinality unbounded.
                    new KeyValuePair<string, object?>("behavior.class", context.Behavior.Id.BehaviorClass.ToString()),
                    new KeyValuePair<string, object?>("event.name", eventName),
                    new KeyValuePair<string, object?>("event.status", Enum.GetName(eventStatus))
                );
            }
        }
    }

}