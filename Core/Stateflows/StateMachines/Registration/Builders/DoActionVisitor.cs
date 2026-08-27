using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

public class DoActionVisitor(ITypeMapper typeMapper) : ActionVisitor
{
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

    public List<Type> EventTypes { get; } = [];
}