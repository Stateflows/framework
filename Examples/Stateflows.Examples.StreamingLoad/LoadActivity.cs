using System.Diagnostics;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Examples.StreamingLoad;

public sealed class LoadActivity : IActivity
{
    public const string Name = "StreamingLoad";
    public const string SettingsKey = "workflow-settings";

    public static void Build(IActivityBuilder activity) => activity
        .AddDefaultInitializer<LoadInitializer>()
        .AddAcceptEventAction<ConfigureWorkflow, ConfigureWorkflowAction>()
        .AddInputPin<WorkToken>(pin => pin.AddFlow<ProcessWorkAction>())
        .AddAction<ProcessWorkAction>(node => node.Reentrant()
            .AddFlow<NormalResult, CompleteNormalAction>()
            .AddFlow<PriorityResult, CompletePriorityAction>())
        .AddAction<CompleteNormalAction>()
        .AddAction<CompletePriorityAction>();
}

public sealed class LoadInitializer(IActivityContext activity) : IDefaultInitializer
{
    public async Task<bool> OnInitializeAsync()
    {
        await activity.Values.SetAsync(LoadActivity.SettingsKey, new WorkflowState(new WorkflowSettings(), 0));
        return true;
    }
}

public sealed class ConfigureWorkflowAction(
    IActivityContext activity, IExecutionContext execution, SafetyLimits limits, WorkerPool workers,
    LoadMetrics metrics) : IAcceptEventActionNode<ConfigureWorkflow>
{
    public async Task ExecuteAsync(ConfigureWorkflow @event, CancellationToken cancellationToken)
    {
        @event.Settings.Validate(limits);
        if (execution.ExecutionMode != ExecutionMode.Transactional || execution.ExecutionTriggerId == null)
            throw new InvalidOperationException("Workflow controls must be delivered transactionally, without a Stream header.");
        var current = await activity.Values.GetOrDefaultAsync<WorkflowState>(LoadActivity.SettingsKey)
            ?? throw new InvalidOperationException("Workflow settings have not been initialized.");
        var revision = current.Revision + 1;
        await activity.Values.SetAsync(LoadActivity.SettingsKey, new WorkflowState(@event.Settings, revision));
        workers.Configure(@event.Settings.Workers);
        metrics.ControlApplied(new AppliedControl(revision, @event.Settings,
            execution.ExecutionMode.ToString(), execution.ExecutionTriggerId));
    }
}

public sealed class ProcessWorkAction(
    IActivityContext activity, IExecutionContext execution, IInputTokens<WorkToken> input,
    IOutputTokens<NormalResult> normal, IOutputTokens<PriorityResult> priority,
    WorkerPool workers) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (execution.ExecutionMode != ExecutionMode.Streamed)
            throw new InvalidOperationException("The load pipeline expects streamed token admission.");
        var state = await ReadStateAsync();
        if (HasNoSyntheticWork(state.Settings))
        {
            using var lease = await workers.EnterAsync(cancellationToken);
            state = await ReadStateAsync();
            if (HasNoSyntheticWork(state.Settings))
            {
                // Zero-work batches need neither per-token tasks nor repeated JSON value deserialization.
                foreach (var token in input)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Route(token, state.Revision, token.Sequence % 100 < state.Settings.PriorityPercent);
                }
                return;
            }
        }
        var results = await Task.WhenAll(input.Select(async token =>
        {
            using var lease = await workers.EnterAsync(cancellationToken);
            var settings = (await ReadStateAsync()).Settings;
            var payload = settings.CpuMilliseconds > 0
                ? await Task.Run(() => Burn(settings, cancellationToken), cancellationToken)
                : new byte[settings.AllocationKiB * 1024];
            if (settings.DelayMilliseconds > 0)
                await Task.Delay(settings.DelayMilliseconds, cancellationToken);
            GC.KeepAlive(payload);
            // Routing reads the latest control after I/O, so ordinary events can steer work already in flight.
            var latest = await ReadStateAsync();
            return (Token: token, latest.Revision, Priority: token.Sequence % 100 < latest.Settings.PriorityPercent);
        }));
        foreach (var result in results)
            Route(result.Token, result.Revision, result.Priority);
    }

    private async Task<WorkflowState> ReadStateAsync()
        => await activity.Values.GetOrDefaultAsync<WorkflowState>(LoadActivity.SettingsKey)
            ?? throw new InvalidOperationException("Workflow settings have not been initialized.");

    private static bool HasNoSyntheticWork(WorkflowSettings settings)
        => settings is { CpuMilliseconds: 0, DelayMilliseconds: 0, AllocationKiB: 0 };

    private void Route(WorkToken token, int revision, bool isPriority)
    {
        if (isPriority)
            priority.Add(new PriorityResult(token, revision));
        else
            normal.Add(new NormalResult(token, revision));
    }

    private static byte[] Burn(WorkflowSettings settings, CancellationToken cancellationToken)
    {
        var payload = new byte[settings.AllocationKiB * 1024];
        var started = Stopwatch.GetTimestamp();
        var value = 17;
        while (Stopwatch.GetElapsedTime(started).TotalMilliseconds < settings.CpuMilliseconds)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var index = 0; index < 512; index++)
                value = unchecked(value * 1664525 + 1013904223);
        }
        if (payload.Length > 0) payload[value & (payload.Length - 1)] = (byte)value;
        return payload;
    }
}

public sealed class CompleteNormalAction(IInputTokens<NormalResult> input, LoadMetrics metrics) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var result in input) metrics.Complete(result.Token, priority: false);
        return Task.CompletedTask;
    }
}

public sealed class CompletePriorityAction(IInputTokens<PriorityResult> input, LoadMetrics metrics) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var result in input) metrics.Complete(result.Token, priority: true);
        return Task.CompletedTask;
    }
}
