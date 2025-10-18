using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

public class DoActivityVisitor(ITypeMapper typeMapper) : ActivityVisitor
{
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

    public List<Type> EventTypes { get; } = [];
}