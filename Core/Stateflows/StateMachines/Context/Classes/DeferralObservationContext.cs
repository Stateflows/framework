using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using System.Linq;

namespace Stateflows.StateMachines.Context.Classes;

internal sealed class DeferralObservationContext(
    RootContext context, Vertex vertex, EventHolder candidate, bool isReevaluation)
    : StateActionContext(context, vertex, Constants.Deferral), IDeferralObservationContext
{
    public string Identifier => Graph.DeferralIdentifier(vertex, vertex.Deferrals
        .Where(pair => pair.Value == vertex.Deferrals[candidate.Name]).Select(pair => pair.Key).Order(StringComparer.Ordinal).First());
    public object Event => candidate.BoxedPayload;
    public string EventName => candidate.Name;
    public Guid EventId => candidate.Id;
    public IDictionary<string, EventHeader> EventHeaders => candidate.Headers;
    public bool IsReevaluation => isReevaluation;
}
