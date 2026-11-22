using System;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities;

/// <summary>A real registration executing within an activity's aggregate operation.</summary>
public interface IActivityOperationObservationContext : IActivityActionContext
{
    /// <summary>Gets the stable registration identifier reported by the definition visitor.</summary>
    string Identifier { get; }
    /// <summary>Gets the identifier of the containing activity, node, or flow.</summary>
    string OwnerId { get; }
    /// <summary>Gets the zero-based registration position within its owning operation.</summary>
    int Order { get; }
    /// <summary>The current delivery, when execution is associated with an event.</summary>
    object? Event { get; }
    /// <summary>Gets the current event name, or null when no delivery is active.</summary>
    string? EventName { get; }
    /// <summary>Gets the current payload token name, or the CLR token name for a typed token.</summary>
    string? TokenName { get; }
    /// <summary>Gets the current delivery identifier, or null when no delivery is active.</summary>
    Guid? EventId { get; }
    /// <summary>Gets the current delivery headers, or null when no delivery is active.</summary>
    IDictionary<string, EventHeader>? EventHeaders { get; }
}

/// <summary>An individual node or activity lifecycle action registration.</summary>
public interface IActivityActionObservationContext : IActivityOperationObservationContext
{
    /// <summary>Gets the semantic kind of the registered action.</summary>
    ActivityOperationKind Operation { get; }
}

/// <summary>Describes an initialization predicate, flow predicate, or short-circuit Boolean group.</summary>
public interface IActivityPredicateObservationContext : IActivityOperationObservationContext
{
    /// <summary>Gets the initialization kind for an initializer; null for a flow predicate or group.</summary>
    ActivityOperationKind? Operation { get; }
    /// <summary>Gets the containing Boolean-group identifier, or null for a root predicate or initializer.</summary>
    string? ParentGuardId { get; }
    /// <summary>Gets the Boolean expression kind; initializers are leaf predicates.</summary>
    ActivityGuardKind Kind { get; }
    /// <summary>Gets the input token type for a flow predicate; null for an initializer.</summary>
    Type? TokenType { get; }
    /// <summary>Gets the input token for a flow predicate; null when no token is associated with the predicate.</summary>
    object? Token { get; }
    /// <summary>Gets the input token envelope identifier for a flow predicate; null for an initializer.</summary>
    Guid? TokenId { get; }
}
