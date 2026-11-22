using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

/// <summary>Collects effective event names and mapped CLR payload types exposed by an embedded action.</summary>
public class DoActionVisitor(ITypeMapper typeMapper) : ActionVisitor
{
    public List<Type> EventTypes { get; } = [];
    public List<string> EventNames { get; } = [];
    public Dictionary<Type, HashSet<string>> EventBindings { get; } = [];

    public override Task CustomEventAddedAsync<TEvent>(string bindingId, string actionName, int actionVersion, IReadOnlyList<string> eventNames, BehaviorStatus[] supportedStatuses)
    {
        if (!EventBindings.TryGetValue(typeof(TEvent), out var bindings))
            EventBindings[typeof(TEvent)] = bindings = [];
        foreach (var name in eventNames)
        {
            bindings.Add(name);
            if (!EventNames.Contains(name)) EventNames.Add(name);
        }
        foreach (var type in typeMapper.GetMappedTypes(typeof(TEvent)))
            if (!EventTypes.Contains(type)) EventTypes.Add(type);
        return Task.CompletedTask;
    }
}
