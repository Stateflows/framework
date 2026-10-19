using System;

namespace Stateflows.Common.Registration.Interfaces;

public interface IResourceBuilder
{
    IResourceBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions);

    /// <summary>
    /// Limits the number of executions waiting for a permit (requires a concurrency limit). Executions above the limit
    /// are rejected with <see cref="EventStatus.Overloaded"/> without executing. Negative values mean unlimited (default);
    /// 0 rejects whenever no permit is immediately available.
    /// </summary>
    IResourceBuilder SetMaxQueuedBehaviorExecutions(int maxQueuedBehaviorExecutions);

    /// <summary>
    /// Limits how long an execution waits for a permit (requires a concurrency limit). Executions that time out
    /// are rejected with <see cref="EventStatus.Overloaded"/> without executing. Default: unlimited.
    /// </summary>
    IResourceBuilder SetMaxQueueWaitTime(TimeSpan maxQueueWaitTime);
}