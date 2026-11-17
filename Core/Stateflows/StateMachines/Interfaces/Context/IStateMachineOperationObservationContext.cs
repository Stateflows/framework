using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>A registered operation executing inside an aggregate machine operation.</summary>
public interface IStateMachineOperationObservationContext : IStateMachineActionContext
{
    /// <summary>The stable registration identifier.</summary>
    string Identifier { get; }
    /// <summary>The owning model-element identifier.</summary>
    string OwnerId { get; }
    /// <summary>The current event payload.</summary>
    object Event { get; }
    /// <summary>The current event name.</summary>
    string EventName { get; }
    /// <summary>The current delivery identifier.</summary>
    Guid EventId { get; }
    /// <summary>The current delivery headers.</summary>
    IDictionary<string, EventHeader> EventHeaders { get; }
}

/// <summary>An individual action registration.</summary>
public interface IStateMachineActionObservationContext : IStateMachineOperationObservationContext
{
    /// <summary>The semantic action kind.</summary>
    StateMachineOperation Operation { get; }
}

/// <summary>An initializer, transition-guard predicate, deferral-guard predicate, or explicit Boolean group.</summary>
public interface IStateMachinePredicateObservationContext : IStateMachineOperationObservationContext
{
    /// <summary>The parent Boolean-group identifier, or null for a root predicate or initializer.</summary>
    string? ParentGuardId { get; }
    /// <summary>The registered Boolean operation.</summary>
    GuardKind Kind { get; }
}
