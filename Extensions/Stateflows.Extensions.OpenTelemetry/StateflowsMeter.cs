using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

public class StateflowsMeter : IHostedService, System.IDisposable
{
    public const string MeterName = "Stateflows";
    /// <summary>Meter of the Stateflows Orleans runtime (persistence, reminder, index, publication and admission failures).</summary>
    public const string OrleansMeterName = "Stateflows.Orleans";
    public Meter Meter { get; } = new(MeterName, "1.0.0");
    public IDictionary<string, Counter<long>> ExecutionCounters { get; } = new ConcurrentDictionary<string, Counter<long>>();
    public IDictionary<string, Histogram<double>> ExecutionDurations { get; } = new ConcurrentDictionary<string, Histogram<double>>();
    public IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }
    
    public StateflowsMeter(IStateflowsResources stateflowsResources)
    {
        ResourcesByBehaviorClass = stateflowsResources.ResourcesByBehaviorClass;
        foreach (var resource in stateflowsResources.Resources)
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
                $"stateflows.resource.{resourceName}.execution.waiting",
                () => resource.WaitingExecutionsCount,
                description: "Behavior executions waiting for a resource permit (does not include Orleans grain mailbox depth)"
            );

#pragma warning disable CS0618
            Meter.CreateObservableGauge(
                $"stateflows.resource.{resourceName}.execution.queueLength",
                () => resource.EventQueueLength,
                description: "Deprecated, use execution.waiting. In-process event channel length plus permit waiters"
            );
#pragma warning restore CS0618
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public void Dispose() => Meter.Dispose();
}