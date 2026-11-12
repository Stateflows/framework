using System.Diagnostics;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Examples.StreamingLoad;

public sealed class LoadCoordinator(
    IBehaviorFactory factory, LoadMetrics metrics, WorkerPool workers, SafetyLimits limits,
    ILogger<LoadCoordinator> logger) : BackgroundService
{
    private readonly SemaphoreSlim commands = new(1, 1);
    private IBehavior? behavior;
    private CancellationTokenSource? generation;
    private Task? pump;
    private CancellationToken stopping;
    private static readonly Dictionary<string, EventHeader> StreamHeaders = new()
    {
        [nameof(Stateflows.Common.Stream)] = new Stateflows.Common.Stream()
    };

    public DashboardSnapshot Snapshot => metrics.Snapshot(workers.Active);
    public SafetyLimits Limits => limits;

    public async Task StartRunAsync(LoadSettings load, WorkflowSettings workflow)
    {
        load.Validate(limits);
        workflow.Validate(limits);
        await commands.WaitAsync(stopping);
        var starting = false;
        try
        {
            if (metrics.State is not ("Idle" or "Completed" or "Stopped") || workers.Active > 0)
                throw new InvalidOperationException("Stop or drain the current run before starting another.");
            starting = true;
            await StopGeneratorAsync();
            await DisposeBehaviorAsync();
            var id = Guid.NewGuid();
            metrics.Begin(id, load, workflow);
            behavior = factory.CreateBehavior(new ActivityId(LoadActivity.Name, id.ToString("N")));
            var initialized = await behavior.SendAsync(new Initialize());
            if (initialized.Status != EventStatus.Initialized)
                throw new InvalidOperationException($"Initialization returned {initialized.Status}.");
            await ConfigureCoreAsync(workflow);
            StartGenerator();
        }
        catch (Exception exception) when (starting)
        {
            ReportFault(exception);
            throw;
        }
        finally { commands.Release(); }
    }

    public async Task ApplyWorkflowAsync(WorkflowSettings settings)
    {
        settings.Validate(limits);
        await commands.WaitAsync(stopping);
        try
        {
            RequireLiveRun();
            await ConfigureCoreAsync(settings);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Transactional workflow control failed.");
            throw;
        }
        finally { commands.Release(); }
    }

    private async Task ConfigureCoreAsync(WorkflowSettings settings)
    {
        var result = await behavior!.SendAsync(new ConfigureWorkflow(settings));
        if (result.Status != EventStatus.Consumed)
            throw new InvalidOperationException($"Workflow control returned {result.Status}; settings were not acknowledged.");
    }

    public async Task ApplyLoadAsync(LoadSettings settings)
    {
        settings.Validate(limits);
        await commands.WaitAsync(stopping);
        try
        {
            RequireLiveRun();
            metrics.SetLoad(settings);
        }
        finally { commands.Release(); }
    }

    public async Task PauseAsync()
    {
        await commands.WaitAsync(stopping);
        try
        {
            if (metrics.State != "Running") throw new InvalidOperationException("Only a running generator can be paused.");
            await StopGeneratorAsync();
            metrics.SetState("Paused");
        }
        finally { commands.Release(); }
    }

    public async Task ResumeAsync()
    {
        await commands.WaitAsync(stopping);
        try
        {
            if (metrics.State != "Paused") throw new InvalidOperationException("Only a paused run can resume.");
            StartGenerator();
        }
        finally { commands.Release(); }
    }

    public async Task DrainAsync()
    {
        await commands.WaitAsync(stopping);
        try
        {
            RequireLiveRun();
            await StopGeneratorAsync();
            metrics.SetState("Draining");
        }
        finally { commands.Release(); }
    }

    public async Task EmergencyStopAsync()
    {
        await commands.WaitAsync(stopping);
        try
        {
            await StopGeneratorAsync();
            if (behavior != null)
            {
                var result = await behavior.FinalizeAsync(FinalizationMode.Immediate);
                if (result.Status is not (EventStatus.Consumed or EventStatus.Cancelled))
                    throw new InvalidOperationException($"Emergency finalization returned {result.Status}.");
            }
            metrics.Abandon();
            metrics.SetState("Stopped");
        }
        catch (Exception exception) { ReportFault(exception); throw; }
        finally { commands.Release(); }
    }

    private void RequireLiveRun()
    {
        if (behavior == null || metrics.State is not ("Running" or "Paused" or "Draining"))
            throw new InvalidOperationException("Start a run first.");
    }

    private void StartGenerator()
    {
        generation = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        metrics.SetState("Running");
        pump = GenerateAsync(generation.Token);
    }

    private async Task StopGeneratorAsync()
    {
        if (generation == null) return;
        await generation.CancelAsync();
        if (pump != null) await pump;
        generation.Dispose();
        generation = null;
        pump = null;
        metrics.SetTarget(0);
    }

    private async Task GenerateAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(20));
        var last = Stopwatch.GetTimestamp();
        var lastAdmission = last;
        double budget = 0;
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var now = Stopwatch.GetTimestamp();
                var seconds = Stopwatch.GetElapsedTime(last, now).TotalSeconds;
                last = now;
                var load = metrics.GetLoad();
                var target = load.RateAt(metrics.Elapsed);
                metrics.SetTarget(target);
                // No catch-up flood after a pause, scheduling delay, or backpressure.
                budget = Math.Min(budget + target * seconds, Math.Max(load.BatchSize, target * 0.1));
                while (budget >= 1 && !cancellationToken.IsCancellationRequested)
                {
                    var batchTarget = Math.Min(load.BatchSize, load.OutstandingLimit);
                    if (budget < batchTarget && Stopwatch.GetElapsedTime(lastAdmission, now) < TimeSpan.FromMilliseconds(100))
                        break;
                    var tokens = metrics.Reserve((int)Math.Min(budget, load.BatchSize));
                    if (tokens.Length == 0)
                    {
                        metrics.Throttled(seconds);
                        budget = 0;
                        break;
                    }
                    var input = new TokensInput<WorkToken>();
                    foreach (var token in tokens) input.Add(token);
                    try
                    {
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var result = await behavior!.SendAsync(input, StreamHeaders);
                            if (result.Status == EventStatus.Accepted)
                            {
                                metrics.Admitted(tokens.Length);
                                lastAdmission = Stopwatch.GetTimestamp();
                                break;
                            }
                            if (result.Status != EventStatus.Overloaded)
                            {
                                metrics.Reject(tokens);
                                throw new InvalidOperationException($"Stream admission returned {result.Status}.");
                            }
                            // Overloaded guarantees no admission. Keep just this bounded batch, not replacement tokens.
                            metrics.AdmissionOverloaded();
                            var waitStarted = Stopwatch.GetTimestamp();
                            try { await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken); }
                            finally { metrics.Throttled(Stopwatch.GetElapsedTime(waitStarted).TotalSeconds); }
                            budget = tokens.Length;
                            last = Stopwatch.GetTimestamp();
                        }
                    }
                    catch
                    {
                        metrics.CancelReservation(tokens);
                        throw;
                    }
                    budget -= tokens.Length;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception exception) { ReportFault(exception); }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stopping = stoppingToken;
        using var process = Process.GetCurrentProcess();
        var timestamp = Stopwatch.GetTimestamp();
        var cpu = process.TotalProcessorTime;
        var allocated = GC.GetTotalAllocatedBytes();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                process.Refresh();
                var now = Stopwatch.GetTimestamp();
                var seconds = Stopwatch.GetElapsedTime(timestamp, now).TotalSeconds;
                var nextCpu = process.TotalProcessorTime;
                var nextAllocated = GC.GetTotalAllocatedBytes();
                metrics.Sample(seconds,
                    Math.Clamp((nextCpu - cpu).TotalSeconds / seconds / Environment.ProcessorCount * 100, 0, 100),
                    process.WorkingSet64 / 1048576d, GC.GetTotalMemory(false) / 1048576d,
                    (nextAllocated - allocated) / seconds / 1048576d,
                    [GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2)]);
                timestamp = now; cpu = nextCpu; allocated = nextAllocated;
                if (metrics.State == "Draining" && metrics.Outstanding == 0)
                    await FinishDrainAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            await StopGeneratorAsync();
            if (behavior != null && metrics.State is not ("Completed" or "Stopped"))
            {
                try { await behavior.FinalizeAsync(FinalizationMode.Immediate); }
                catch (Exception exception) { logger.LogError(exception, "Failed to finalize benchmark during host shutdown."); }
            }
            await DisposeBehaviorAsync();
        }
    }

    private async Task FinishDrainAsync(CancellationToken cancellationToken)
    {
        await commands.WaitAsync(cancellationToken);
        try
        {
            if (metrics.State != "Draining" || metrics.Outstanding != 0) return;
            var result = await behavior!.FinalizeAsync(FinalizationMode.Queued);
            if (result.Status != EventStatus.Consumed)
                throw new InvalidOperationException($"Drain finalization returned {result.Status}.");
            metrics.SetState("Completed");
        }
        catch (Exception exception) { ReportFault(exception); }
        finally { commands.Release(); }
    }

    private void ReportFault(Exception exception)
    {
        logger.LogError(exception, "Streaming benchmark failed; use emergency stop before restarting.");
        metrics.Fault(exception);
    }

    private async Task DisposeBehaviorAsync()
    {
        if (behavior is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
        else behavior?.Dispose();
        behavior = null;
    }
}
