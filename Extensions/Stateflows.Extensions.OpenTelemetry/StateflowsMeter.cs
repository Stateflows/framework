using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

/// <summary>Exposes Stateflows execution metrics and their meter to the OpenTelemetry pipeline.</summary>
public class StateflowsMeter : IHostedService, System.IDisposable
{
    /// <summary>The OpenTelemetry meter name used for Stateflows behavior execution metrics.</summary>
    public const string MeterName = "Stateflows";
    /// <summary>Meter of the Stateflows Orleans runtime (persistence, reminder, index, publication and admission failures).</summary>
    public const string OrleansMeterName = "Stateflows.Orleans";
    /// <summary>Gets the meter that owns the Stateflows and Orleans instruments.</summary>
    public Meter Meter { get; } = new(MeterName, "1.0.0");
    /// <summary>Gets execution counters keyed by resource name.</summary>
    public IDictionary<string, Counter<long>> ExecutionCounters { get; } = new ConcurrentDictionary<string, Counter<long>>();
    /// <summary>Gets execution duration histograms keyed by resource name.</summary>
    public IDictionary<string, Histogram<double>> ExecutionDurations { get; } = new ConcurrentDictionary<string, Histogram<double>>();
    /// <summary>Gets the resources indexed by their behavior classes.</summary>
    public IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }
    
    /// <summary>Creates metric instruments for all configured Stateflows resources.</summary>
    /// <param name="stateflowsResources">The resources whose executions are measured.</param>
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

    /// <summary>Starts the metrics service; instruments are available on the meter immediately.</summary>
    /// <param name="cancellationToken">A token that can cancel the start operation.</param>
    /// <returns>A completed task.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>Stops the metrics service.</summary>
    /// <param name="cancellationToken">A token that can cancel the stop operation.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    /// <summary>Disposes the meter and its instruments.</summary>
    public void Dispose() => Meter.Dispose();
}