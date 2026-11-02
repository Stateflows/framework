using System;
using Stateflows.Common;

namespace Stateflows.Activities.Streams;

/// <summary>Reports an unhandled failure in one admitted Activity stream event.</summary>
[ImmediatePublish]
public sealed class ActivityStreamFailure
{
    /// <summary>The Activity instance whose streamed execution failed.</summary>
    public BehaviorId BehaviorId { get; set; }

    /// <summary>The event identifier of the admitted input or live timer.</summary>
    public Guid EventId { get; set; }

    /// <summary>The event type name of the failed invocation.</summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>The exception types reported by the failed invocation.</summary>
    public string[] ExceptionTypes { get; set; } = [];
}
