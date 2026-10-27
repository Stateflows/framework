using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;
using Stateflows.Common;

namespace Stateflows.StateMachines.Registration.Builders;

/// <summary>Collects the event definitions and mapped payload types accepted by an embedded do activity.</summary>
/// <param name="typeMapper">Resolves mapped event types accepted by activity nodes.</param>
public class DoActivityVisitor(ITypeMapper typeMapper) : ActivityVisitor
{
    /// <summary>The distinct accepted event definitions collected by event name.</summary>
    public List<EventDefinition> EventDefinitions { get; } = [];
    /// <inheritdoc/>
    public override Task AcceptEventNodeAddedAsync(EventDefinition definition, string activityName, int activityVersion,
        string nodeName, string parentNodeName = null)
    {
        if (!EventDefinitions.Exists(e => e.Name == definition.Name)) EventDefinitions.Add(definition);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName,
        string parentNodeName = null)
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

    /// <summary>The distinct mapped CLR payload types accepted by visited event-accepting nodes.</summary>
    public List<Type> EventTypes { get; } = [];
}