using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

public class SubmachineVisitor(ITypeMapper typeMapper) : StateMachineVisitor
{
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

    public List<Type> EventTypes { get; } = [];
}