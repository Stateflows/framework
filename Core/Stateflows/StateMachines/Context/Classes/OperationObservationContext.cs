using System;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.StateMachines.Models;

namespace Stateflows.StateMachines.Context.Classes;

internal abstract class OperationObservationContext(RootContext context, OperationRegistration registration)
    : StateMachineActionContext(context), IStateMachineOperationObservationContext
{
    public string Identifier => registration.Identifier;
    public string OwnerId => registration.OwnerId;
    public object Event => context.EventHolder.BoxedPayload;
    public string EventName => context.EventHolder.Name;
    public Guid EventId => context.EventHolder.Id;
    public IDictionary<string, EventHeader> EventHeaders => context.EventHolder.Headers;
}

internal sealed class ActionObservationContext(RootContext context, OperationRegistration registration)
    : OperationObservationContext(context, registration), IStateMachineActionObservationContext
{
    public StateMachineOperation Operation => registration.Operation!.Value;
}

internal sealed class PredicateObservationContext(RootContext context, OperationRegistration registration)
    : OperationObservationContext(context, registration), IStateMachinePredicateObservationContext
{
    public string? ParentGuardId => registration.ParentGuardId;
    public GuardKind Kind => registration.Kind;
}
