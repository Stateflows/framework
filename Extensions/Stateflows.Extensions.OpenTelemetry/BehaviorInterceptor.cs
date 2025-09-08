using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

public class BehaviorInterceptor : IBehaviorInterceptor
{
    private Stopwatch _stopwatch;
    private static double _lastDuration;

    public Task AfterHydrateAsync(IBehaviorActionContext context)
        => Task.CompletedTask;

    public Task BeforeDehydrateAsync(IBehaviorActionContext context)
        => Task.CompletedTask;

    public bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
    {
        _stopwatch = Stopwatch.StartNew();
        
        return true;
    }

    public void AfterProcessEvent<TEvent>(IEventContext<TEvent> context, EventStatus eventStatus)
    {
        _stopwatch.Stop();
        
        var noTracing =
            context.Event!.GetType().GetCustomAttributes<NoTracingAttribute>().Any() ||
            context.Headers.Any(h => h is NoTracing);

        if (noTracing)
        {
            return;
        }

        var eventName = Event.GetName(context.Event.GetType());
        
        StateflowsMeter.ExecutionDuration.Record(
            _stopwatch.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("behavior.class", context.Behavior.Id.BehaviorClass),
            new KeyValuePair<string, object?>("behavior.id", context.Behavior.Id),
            new KeyValuePair<string, object?>("behavior.id.instance", context.Behavior.Id.Instance),
            new KeyValuePair<string, object?>("event.name", eventName),
            new KeyValuePair<string, object?>("event.status", Enum.GetName(eventStatus))
        );
        
        StateflowsMeter.ExecutionCounter.Add(
            1,
            new KeyValuePair<string, object?>("behavior.class", context.Behavior.Id.BehaviorClass),
            new KeyValuePair<string, object?>("behavior.id", context.Behavior.Id),
            new KeyValuePair<string, object?>("behavior.id.instance", context.Behavior.Id.Instance),
            new KeyValuePair<string, object?>("event.name", eventName),
            new KeyValuePair<string, object?>("event.status", Enum.GetName(eventStatus))
        );
    }
}