using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows.Extensions.OpenTelemetry;

public class StateflowsMeter : IHostedService
{
    public static Meter Meter = new("Stateflows", "1.0.0");
    public static Counter<long> ExecutionCounter = Meter.CreateCounter<long>(
        "stateflows.behavior.execution.count",
        "count",
        description: "Counts the number of behavior executions"
    );
    
    public static Histogram<double> ExecutionDuration = Meter.CreateHistogram<double>(
        "stateflows.behavior.execution.duration",
        "ms",
        description: "Duration of behavior execution",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = [0.01, 0.05, 0.1, 0.5, 1, 5] }
    );
    
    public StateflowsMeter(IStateflowsTelemetry stateflowsTelemetry)
    {
        Meter.CreateObservableGauge(
            "stateflows.behavior.execution.running",
            () => stateflowsTelemetry.BehaviorExecutionsCount,
            description: "Active behavior executions"
        );
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}