using System;

namespace Stateflows.Common.Registration.Interfaces;

/// <summary>Configures concurrency and queue limits for a named behavior resource.</summary>
public interface IResourceBuilder
{
    /// <summary>Sets the maximum number of behavior executions that can run concurrently.</summary>
    /// <param name="maxConcurrentBehaviorExecutions">The maximum concurrent executions.</param>
    /// <returns>This resource builder for fluent configuration.</returns>
    IResourceBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions);

    /// <summary>
    /// Limits the number of executions waiting for a permit (requires a concurrency limit). Executions above the limit
    /// are rejected with <see cref="EventStatus.Overloaded"/> without executing. Negative values mean unlimited (default);
    /// 0 rejects whenever no permit is immediately available.
    /// </summary>
    /// <param name="maxQueuedBehaviorExecutions">Maximum queued executions; a negative value means unlimited.</param>
    /// <returns>This resource builder for fluent configuration.</returns>
    IResourceBuilder SetMaxQueuedBehaviorExecutions(int maxQueuedBehaviorExecutions);

    /// <summary>
    /// Limits how long an execution waits for a permit (requires a concurrency limit). Executions that time out
    /// are rejected with <see cref="EventStatus.Overloaded"/> without executing. Default: unlimited.
    /// </summary>
    /// <param name="maxQueueWaitTime">Maximum time an execution waits for a concurrency permit.</param>
    /// <returns>This resource builder for fluent configuration.</returns>
    IResourceBuilder SetMaxQueueWaitTime(TimeSpan maxQueueWaitTime);
}