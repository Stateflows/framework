using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine.Interfaces;

public enum ResourceRejectionReason
{
    None,
    /// <summary>The configured maximum number of executions waiting for a permit was reached.</summary>
    QueueFull,
    /// <summary>No permit became available within the configured maximum queue wait time.</summary>
    WaitTimeout,
    /// <summary>The host is shutting down; waiting and new executions are rejected so callers can retry elsewhere.</summary>
    ShuttingDown
}

public readonly struct ResourceAdmission
{
    private ResourceAdmission(IDisposable? lease, ResourceRejectionReason rejection)
    {
        Lease = lease;
        Rejection = rejection;
    }

    public IDisposable? Lease { get; }
    public ResourceRejectionReason Rejection { get; }
    public bool IsAdmitted => Rejection == ResourceRejectionReason.None;

    public static ResourceAdmission Admitted(IDisposable lease) => new(lease, ResourceRejectionReason.None);
    public static ResourceAdmission Rejected(ResourceRejectionReason reason) => new(null, reason);
}

public interface IStateflowsResource
{
    string Name { get; }
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

    int BehaviorExecutionsCount { get; }

    /// <summary>Waits for a permit without admission limits.</summary>
    ValueTask<IDisposable?> AcquireAsync(CancellationToken? cancellationToken = null);

    /// <summary>Waits for a permit, honoring the configured queue limit, queue wait time and shutdown.</summary>
    ValueTask<ResourceAdmission> TryAcquireAsync(CancellationToken cancellationToken = default);
}

public interface IStateflowsResources
{
    IEnumerable<IStateflowsResource> Resources { get; }
    IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }

    /// <summary>Rejects current permit waiters and all further admissions with <see cref="ResourceRejectionReason.ShuttingDown"/>.</summary>
    void BeginShutdown();
}