using Microsoft.Extensions.Logging;
using Stateflows.Common.Engine.Interfaces;

namespace Stateflows;

/// <summary>
/// Releases resource permit waiters as soon as the silo starts stopping: they return <see cref="EventStatus.Overloaded"/>
/// so callers can retry on another silo. Executions that already hold a permit run to completion and commit before
/// the silo proceeds to deactivate grains, so their persistence, publication and reminder calls still reach live grains.
/// Draining is bounded by the silo stop cancellation (graceful shutdown timeout).
/// </summary>
internal sealed class ResourceShutdownParticipant(
    IStateflowsResources resources,
    ILogger<ResourceShutdownParticipant> logger
) : ILifecycleParticipant<ISiloLifecycle>
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(20);

    public void Participate(ISiloLifecycle lifecycle)
        => lifecycle.Subscribe(
            nameof(ResourceShutdownParticipant),
            ServiceLifecycleStage.Last,
            _ => Task.CompletedTask,
            OnStopAsync);

    private async Task OnStopAsync(CancellationToken cancellationToken)
    {
        var waiting = resources.Resources.Sum(resource => resource.WaitingExecutionsCount);
        logger.LogInformation(
            "Silo is stopping: rejecting {Waiting} waiting Stateflows executions; {Running} running executions complete.",
            waiting, RunningCount());
        resources.BeginShutdown();

        try
        {
            while (RunningCount() > 0)
            {
                await Task.Delay(PollInterval, cancellationToken);
            }

            logger.LogInformation("Silo is stopping: all running Stateflows executions completed.");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Silo is stopping: {Running} Stateflows executions were still running when the shutdown timeout elapsed.", RunningCount());
        }
    }

    private int RunningCount()
        => resources.Resources.Sum(resource => resource.BehaviorExecutionsCount);
}