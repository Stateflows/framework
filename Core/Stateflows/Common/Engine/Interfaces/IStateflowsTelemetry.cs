using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine.Interfaces;

public interface IStateflowsResource
{
    string Name { get; }
    int MaxConcurrentBehaviorExecutions { get; }
    int EventQueueLength { get; }
    int BehaviorExecutionsCount { get; }

    ValueTask<IDisposable?> AcquireAsync(CancellationToken? cancellationToken = null);
}

public interface IStateflowsTelemetry
{
    IEnumerable<IStateflowsResource> Resources { get; }
    IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }
}