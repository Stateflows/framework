using System.Collections.Generic;
using System.Linq;
using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Registration.Builders;

namespace Stateflows.Common;

internal sealed class StateflowsResources : IStateflowsResources
{
    internal IReadOnlyDictionary<string, Resource> ResourcesByName { get; }

    public StateflowsResources(StateflowsBuilder builder)
    {
        // Each service provider (silo) gets its own permits, counters and shutdown state.
        ResourcesByName = builder.ResourceNames.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.CreateInstance());
        ResourcesByBehaviorClass = builder.ResourcesByBehaviorClass.ToDictionary(
            pair => pair.Key,
            pair => (IStateflowsResource)ResourcesByName[pair.Value.Name]);
    }

    public IEnumerable<IStateflowsResource> Resources => ResourcesByName.Values;

    public IReadOnlyDictionary<BehaviorClass, IStateflowsResource> ResourcesByBehaviorClass { get; }

    public void BeginShutdown()
    {
        foreach (var resource in ResourcesByName.Values)
        {
            resource.BeginShutdown();
        }
    }
}