namespace Stateflows;

/// <summary>
/// Configures persistence for the Orleans Stateflows runtime.
/// </summary>
public sealed class StateflowsRuntimeOptions
{
    /// <summary>
    /// Maximum queued streamed batches plus in-flight batch executions per Activity activation.
    /// A batch execution can complete after transferring input into an active interleaving invocation,
    /// before its partitions finish. This does not bound the downstream interleaved backlog.
    /// </summary>
    public int MaxQueuedActivityStreamEvents { get; set; } = 128;

    /// <summary>Maximum time a Checkpoint request waits for active Activity stream work to reach an invocation boundary.</summary>
    public TimeSpan ActivityStreamBarrierTimeout { get; set; } = Timeout.InfiniteTimeSpan;

    /// <summary>Test seam: leaves checkpointed ready Activity work suspended (as after a crash) until the next activation recovers it.</summary>
    internal bool SuspendActivityCheckpointResumeForTesting { get; set; }

    /// <summary>Test seam: observes that a Checkpoint request has stopped new Activity firings.</summary>
    internal Action<Stateflows.BehaviorId>? ActivityCheckpointPauseRequestedForTesting { get; set; }

    /// <summary>
    /// The Orleans grain storage provider used for all Stateflows persistent state.
    /// Null selects Orleans' default provider. An explicit name must be nonempty.
    /// </summary>
    public string? StorageProviderName { get; set; }
}
