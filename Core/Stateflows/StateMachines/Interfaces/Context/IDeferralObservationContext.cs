using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Describes a candidate event and the state evaluating its deferral.</summary>
/// <remarks>
/// Used by guard observers, enqueue observers, and deferral-guard exception handlers.
/// During reevaluation the event properties describe the queued candidate, not the
/// newer delivery that triggered reevaluation. That candidate remains the active
/// execution event until guard failure reporting completes; the previous execution
/// event is restored when evaluation exits. Observers must not change behavior state
/// or the candidate payload and headers.
/// </remarks>
public interface IDeferralObservationContext : IStateMachineActionContext
{
    /// <summary>The stable identity of the matching deferral rule, shared with definition visitation.</summary>
    string Identifier { get; }
    /// <summary>The state that owns the matching deferral rule.</summary>
    IStateContext State { get; }

    /// <summary>The candidate event payload.</summary>
    object Event { get; }

    /// <summary>The candidate event name.</summary>
    string EventName { get; }

    /// <summary>The candidate delivery identifier, preserved when a queued event is reevaluated.</summary>
    Guid EventId { get; }

    /// <summary>The candidate event headers.</summary>
    IDictionary<string, EventHeader> EventHeaders { get; }

    /// <summary>Whether an existing queued candidate is being reevaluated rather than considered for initial enqueueing.</summary>
    bool IsReevaluation { get; }
}
