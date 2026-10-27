using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine.Interfaces;

/// <summary>Describes why an execution could not be admitted to a resource.</summary>
public enum ResourceRejectionReason
{
    /// <summary>The execution was admitted.</summary>
    None,
    /// <summary>The configured maximum number of executions waiting for a permit was reached.</summary>
    QueueFull,
    /// <summary>No permit became available within the configured maximum queue wait time.</summary>
    WaitTimeout,
    /// <summary>The host is shutting down; waiting and new executions are rejected so callers can retry elsewhere.</summary>
    ShuttingDown
}

/// <summary>Contains the result of a resource admission attempt.</summary>
public readonly struct ResourceAdmission
{
    private ResourceAdmission(IDisposable? lease, ResourceRejectionReason rejection)
    {
        Lease = lease;
        Rejection = rejection;
    }

    /// <summary>Gets the lease that releases an admitted permit, or <see langword="null"/> when rejected.</summary>
    public IDisposable? Lease { get; }
    /// <summary>Gets the reason admission was rejected, or <see cref="ResourceRejectionReason.None"/> if admitted.</summary>
    public ResourceRejectionReason Rejection { get; }
    /// <summary>Gets whether a permit was admitted.</summary>
    public bool IsAdmitted => Rejection == ResourceRejectionReason.None;

    /// <summary>Creates a successful admission result.</summary>
    /// <param name="lease">The disposable permit lease.</param>
    /// <returns>An admitted result containing the lease.</returns>
    public static ResourceAdmission Admitted(IDisposable lease) => new(lease, ResourceRejectionReason.None);
    /// <summary>Creates a rejected admission result.</summary>
    /// <param name="reason">The reason the execution was rejected.</param>
    /// <returns>A rejected result without a lease.</returns>
    public static ResourceAdmission Rejected(ResourceRejectionReason reason) => new(null, reason);
}

/// <summary>Describes execution limits, metrics, and permit acquisition for a resource.</summary>
public interface IStateflowsResource
{
    /// <summary>Gets the configured resource name.</summary>
    string Name { get; }
    /// <summary>Gets the maximum number of behavior executions allowed concurrently.</summary>
    int MaxConcurrentBehaviorExecutions { get; }

    /// <summary>Maximum number of executions waiting for a permit; -1 means unlimited.</summary>
    int MaxQueuedBehaviorExecutions { get; }

    /// <summary>Maximum time an execution waits for a permit; null means unlimited.</summary>
    TimeSpan? MaxQueueWaitTime { get; }

    /// <summary>
    /// Legacy queue length: in-process event channel length plus permit waiters.
    /// </summary>
    [Obsolete("Use WaitingExecutionsCount; under Orleans this value does not include grain mailbox depth.")]
    int EventQueueLength { get; }

    /// <summary>Number of executions currently waiting for a permit of this resource (not Orleans mailbox depth).</summary>
    int WaitingExecutionsCount { get; }

    /// <summary>Gets the number of behavior executions currently holding a permit.</summary>
    int BehaviorExecutionsCount { get; }

    /// <summary>Waits for a permit without applying the configured queue admission limits.</summary>
    /// <param name="cancellationToken">An optional token that cancels the wait.</param>
    /// <returns>A disposable permit lease, or <see langword="null"/> if no lease was acquired.</returns>
    ValueTask<IDisposable?> AcquireAsync(CancellationToken? cancellationToken = null);

    /// <summary>Waits for a permit, honoring the configured queue limit, queue wait time and shutdown.</summary>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>An admission result containing the permit lease or rejection reason.</returns>
    ValueTask<ResourceAdmission> TryAcquireAsync(CancellationToken cancellationToken = default);
}

/// <summary>Exposes the execution resources configured by the host.</summary>
public interface IStateflowsResources
{
    /// <summary>Gets the configured resources.</summary>
    IEnumerable<IStateflowsResource> Resources { get; }
    /// <summary>Gets resources indexed by the behavior class they govern.</summary>
    IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }

    /// <summary>Rejects current permit waiters and all further admissions with <see cref="ResourceRejectionReason.ShuttingDown"/>.</summary>
    void BeginShutdown();
}