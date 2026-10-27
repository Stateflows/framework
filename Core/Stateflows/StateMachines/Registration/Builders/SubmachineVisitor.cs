using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;
using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Builders;

/// <summary>Collects event metadata accepted by transitions and do behaviors in an embedded machine.</summary>
/// <param name="typeMapper">Resolves the mapped CLR types of transition triggers.</param>
public class SubmachineVisitor(ITypeMapper typeMapper) : StateMachineVisitor
{
    /// <summary>The distinct event definitions collected by registered event name.</summary>
    public List<EventDefinition> EventDefinitions { get; } = [];
    /// <inheritdoc/>
    public override Task DoBehaviorAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
        string stateName, string behaviorName, string behaviorType)
    {
        if (!EventDefinitions.Exists(e => e.Name == definition.Name)) EventDefinitions.Add(definition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task TransitionAddedAsync(EventDefinition definition, string stateMachineName, int stateMachineVersion,
        string sourceVertexName, string targetVertexName = null, bool isElse = false)
    {
        if (!EventDefinitions.Exists(e => e.Name == definition.Name)) EventDefinitions.Add(definition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
        string targetVertexName = null, bool isElse = false)
    {
        var eventTypes = typeMapper.GetMappedTypes(typeof(TEvent));
        foreach (var eventType in eventTypes)
        {
            if (!EventTypes.Contains(eventType))
            {
                EventTypes.Add(eventType);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>The distinct mapped CLR payload types collected from visited transitions.</summary>
    public List<Type> EventTypes { get; } = [];
}