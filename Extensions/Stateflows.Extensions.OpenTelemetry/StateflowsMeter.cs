using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

public class StateflowsMeter : IHostedService
{
    public static Meter Meter = new("Stateflows", "1.0.0");
    public static IDictionary<string, Counter<long>> ExecutionCounters = new Dictionary<string, Counter<long>>();
    public static IDictionary<string, Histogram<double>> ExecutionDurations = new Dictionary<string, Histogram<double>>();
    public static IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass;
    
    public StateflowsMeter(IStateflowsTelemetry stateflowsTelemetry)
    {
        ResourcesByBehaviorClass = stateflowsTelemetry.ResourcesByBehaviorClass;
        foreach (var resource in stateflowsTelemetry.Resources)
        {
            var resourceName = resource.Name == ""
                ? "default"
                : resource.Name;
            
            ExecutionCounters.Add(resource.Name, Meter.CreateCounter<long>(
                $"stateflows.resource.{resourceName}.execution.count",
                "count",
                description: "Counts the number of behavior executions"
            ));
            
            ExecutionDurations.Add(resource.Name, Meter.CreateHistogram<double>(
                $"stateflows.resource.{resourceName}.execution.duration",
                "ms",
                description: "Duration of behavior execution",
                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [0.01, 0.05, 0.1, 0.5, 1, 5] }
            ));
            
            Meter.CreateObservableGauge(
                $"stateflows.resource.{resourceName}.execution.running",
                () => resource.BehaviorExecutionsCount,
                description: "Active behavior executions"
            );
        
            Meter.CreateObservableGauge(
                $"stateflows.resource.{resourceName}.execution.queueLength",
                () => resource.EventQueueLength,
                description: "Queued events"
            );
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}