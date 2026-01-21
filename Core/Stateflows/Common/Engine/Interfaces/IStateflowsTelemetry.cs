using System.Collections.Generic;

namespace Stateflows.Common.Engine.Interfaces;

public interface IStateflowsResource
{
    string Name { get; }
    int EventQueueLength { get; }
    int BehaviorExecutionsCount { get; }
}

public interface IStateflowsTelemetry
{
    IEnumerable<IStateflowsResource> Resources { get; }
    IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }
}