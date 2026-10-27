using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

/// <summary>Collects event definitions and mapped CLR event types exposed by an embedded do action.</summary>
/// <param name="typeMapper">Resolves mapped event types accepted by the action.</param>
public class DoActionVisitor(ITypeMapper typeMapper) : ActionVisitor
{
    /// <summary>The distinct event definitions collected by registered event name.</summary>
    public List<EventDefinition> EventDefinitions { get; } = [];
    /// <inheritdoc/>
    public override Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion,
        BehaviorStatus[] supportedStatuses)
    {
        if (!EventDefinitions.Exists(e => e.Name == definition.Name)) EventDefinitions.Add(definition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
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

    // public override Task TransitionAddedAsync<TEvent>(string stateMachineName, int stateMachineVersion, string sourceVertexName,
    //     string targetVertexName = null, bool isElse = false)
    // {
    //     var eventTypes = typeMapper.GetMappedTypes(typeof(TEvent));
    //     foreach (var eventType in eventTypes)
    //     {
    //         if (!EventTypes.Contains(eventType))
    //         {
    //             EventTypes.Add(eventType);
    //         }
    //     }
    //
    //     return Task.CompletedTask;
    // }

    /// <summary>The distinct mapped CLR payload types collected from the action's custom events.</summary>
    public List<Type> EventTypes { get; } = [];
}