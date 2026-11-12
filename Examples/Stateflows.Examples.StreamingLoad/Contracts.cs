namespace Stateflows.Examples.StreamingLoad;

public enum LoadProfile { Steady, Ramp, Burst, Wave }

public sealed record SafetyLimits
{
    public int MaxTokensPerSecond { get; init; } = 100_000;
    public int MaxWorkers { get; init; } = 64;
    public int MaxOutstanding { get; init; } = 100_000;

    public void Validate()
    {
        if (MaxTokensPerSecond is < 1 or > 100_000 || MaxWorkers is < 1 or > 64 ||
            MaxOutstanding is < 1 or > 100_000)
            throw new ArgumentException("Safety limits must be within 1-100000 tokens/s, 1-64 workers, and 1-100000 outstanding.");
    }
}

public sealed record LoadSettings(
    LoadProfile Profile = LoadProfile.Steady,
    int TokensPerSecond = 1_000,
    int BatchSize = 64,
    int OutstandingLimit = 4_096,
    int PeriodSeconds = 20)
{
    public void Validate(SafetyLimits limits)
    {
        if (!Enum.IsDefined(Profile) || TokensPerSecond < 1 || TokensPerSecond > limits.MaxTokensPerSecond ||
            BatchSize is < 1 or > 256 || OutstandingLimit < 1 || OutstandingLimit > limits.MaxOutstanding ||
            PeriodSeconds is < 2 or > 300)
            throw new ArgumentException($"Load requires rate 1-{limits.MaxTokensPerSecond}, batch 1-256, " +
                $"outstanding 1-{limits.MaxOutstanding}, and period 2-300 seconds.");
    }

    public double RateAt(TimeSpan elapsed)
    {
        var phase = (elapsed.TotalSeconds % PeriodSeconds) / PeriodSeconds;
        var factor = Profile switch
        {
            LoadProfile.Steady => 1,
            LoadProfile.Ramp => 0.1 + 0.9 * phase,
            LoadProfile.Burst => phase < 0.2 ? 1 : 0.1,
            LoadProfile.Wave => 0.55 - 0.45 * Math.Cos(2 * Math.PI * phase),
            _ => throw new ArgumentOutOfRangeException(nameof(Profile))
        };
        return TokensPerSecond * factor;
    }
}

public sealed record WorkflowSettings(
    int Workers = 4,
    int CpuMilliseconds = 1,
    int DelayMilliseconds = 10,
    int AllocationKiB = 4,
    int PriorityPercent = 20)
{
    public void Validate(SafetyLimits limits)
    {
        if (Workers < 1 || Workers > limits.MaxWorkers || CpuMilliseconds is < 0 or > 20 ||
            DelayMilliseconds is < 0 or > 2_000 || AllocationKiB is < 0 or > 256 ||
            PriorityPercent is < 0 or > 100)
            throw new ArgumentException($"Workflow requires workers 1-{limits.MaxWorkers}, CPU 0-20 ms, " +
                "delay 0-2000 ms, allocation 0-256 KiB, and priority 0-100%.");
    }
}

public sealed record ConfigureWorkflow(WorkflowSettings Settings);
public sealed record WorkflowState(WorkflowSettings Settings, int Revision);
public sealed record LoadScenario(string Name, LoadSettings Load, WorkflowSettings Workflow)
{
    public static LoadScenario Throughput { get; } = new("Framework throughput",
        new LoadSettings(TokensPerSecond: 20_000, BatchSize: 256),
        new WorkflowSettings(Workers: 16, CpuMilliseconds: 0, DelayMilliseconds: 0, AllocationKiB: 0));
    public static LoadScenario Synthetic { get; } = new("Synthetic CPU + I/O", new(), new());
}

public sealed record WorkToken(Guid RunId, long Sequence, long CreatedTimestamp);
public sealed record NormalResult(WorkToken Token, int Revision);
public sealed record PriorityResult(WorkToken Token, int Revision);
public sealed record AppliedControl(int Revision, WorkflowSettings Settings, string Mode, Guid? TriggerId);
public sealed record HistoryPoint(double Seconds, double AdmittedPerSecond, double CompletedPerSecond,
    double CpuPercent, double WorkingSetMiB, int Outstanding);

public sealed record DashboardSnapshot(
    string State, string? Error, Guid? RunId, LoadSettings Load, WorkflowSettings Workflow,
    AppliedControl? LastControl, long Admitted, long Completed, long Normal, long Priority,
    long Rejected, long OverloadedAdmissions, long Abandoned, int Outstanding, int ActiveWorkers, int PeakOutstanding,
    double TargetRate, double AdmittedPerSecond, double CompletedPerSecond,
    double MeanLatencyMilliseconds, double P95LatencyMilliseconds,
    double CpuPercent, double WorkingSetMiB, double ManagedHeapMiB, double AllocatedMiBPerSecond,
    int Gen0Collections, int Gen1Collections, int Gen2Collections,
    double ThrottledSeconds, double ElapsedSeconds, HistoryPoint[] History);
