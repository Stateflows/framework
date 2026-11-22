using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Context.Classes;

internal sealed class HostedBehaviorObservationContext(RootContext context, Vertex vertex, BehaviorId destination, EventHolder delivery)
    : StateActionContext(context, vertex, Constants.Do), IHostedBehaviorObservationContext
{
    public string Identifier => $"{vertex.DefinitionIdentifier}/hosted";
    public BehaviorId EmbeddedBehaviorId => destination;
    public object Event => delivery.BoxedPayload;
    public string EventName => delivery.Name;
    public Guid EventId => delivery.Id;
    public IDictionary<string, EventHeader> EventHeaders => delivery.Headers;
}
