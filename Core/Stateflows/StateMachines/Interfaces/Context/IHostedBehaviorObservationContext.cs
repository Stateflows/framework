using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Describes scheduling a delivery to a state's embedded behavior, not completion of that behavior.</summary>
public interface IHostedBehaviorObservationContext : IStateMachineActionContext
{
    /// <summary>The stable hosted-behavior relationship identifier.</summary>
    string Identifier { get; }
    /// <summary>The owning state.</summary>
    IStateContext State { get; }
    /// <summary>The destination instance.</summary>
    BehaviorId EmbeddedBehaviorId { get; }
    /// <summary>The delivered payload.</summary>
    object Event { get; }
    /// <summary>The delivered event name.</summary>
    string EventName { get; }
    /// <summary>The delivery identifier.</summary>
    Guid EventId { get; }
    /// <summary>The delivered headers.</summary>
    IDictionary<string, EventHeader> EventHeaders { get; }
}
