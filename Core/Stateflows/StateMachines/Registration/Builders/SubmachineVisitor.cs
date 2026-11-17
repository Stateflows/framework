using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

/// <summary>Collects accepted events from an embedded machine's finalized definition.</summary>
public class SubmachineVisitor(ITypeMapper typeMapper) : StateMachineVisitor
{
    /// <summary>Distinct effective CLR payload types.</summary>
    public List<Type> EventTypes { get; } = [];
    /// <summary>Distinct accepted event names, including structural events.</summary>
    public HashSet<string> EventNames { get; } = [];
    /// <summary>Declared CLR event types with their effective names.</summary>
    public Dictionary<Type, HashSet<string>> EventBindings { get; } = [];

    private Task Collect<TEvent>(IReadOnlyList<string> names)
    {
        EventNames.UnionWith(names);
        if (!EventBindings.TryGetValue(typeof(TEvent), out var binding)) EventBindings[typeof(TEvent)] = binding = [];
        binding.UnionWith(names);
        if (typeof(TEvent) != typeof(Stateflows.Common.StructuralEvent))
            foreach (var type in typeMapper.GetMappedTypes(typeof(TEvent)))
                if (!EventTypes.Contains(type)) EventTypes.Add(type);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task TransitionAddedAsync<TEvent>(string id, string name, int version, string sourceId,
        string? targetId, bool isElse, bool isLocal, bool polymorphic, IReadOnlyList<string> triggerNames)
        => Collect<TEvent>(triggerNames);

    /// <inheritdoc/>
    public override Task HostedBehaviorEventAddedAsync<TEvent>(string id, string name, int version,
        string hostedBehaviorId, IReadOnlyList<string> eventNames) => Collect<TEvent>(eventNames);
}
