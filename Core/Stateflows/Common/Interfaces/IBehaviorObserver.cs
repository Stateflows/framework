using System;
using System.Threading.Tasks;

namespace Stateflows.Common;

/// <summary>Observes operations initiated during a Stateflows behavior execution.</summary>
/// <remarks>
/// Async continuations must be called and awaited exactly once; their result and failure
/// cannot be replaced. Synchronous continuations must be called exactly once.
/// Completion describes the operation boundary, not a durable commit.
/// External callers and detached work after execution are not observed.
/// Observers run in service-registration order, with the first registered outermost.
/// These hooks observe calls made by a behavior, not operations performed inside the target.
/// </remarks>
public interface IBehaviorObserver
{
    /// <summary>Wraps an awaited send and exposes the unchanged delivery result.</summary>
    /// <param name="callContext">The initiating behavior, target, event envelope, and optional containing entity operation.</param>
    /// <param name="next">The remaining observers and awaited send; call and await exactly once to inspect its result.</param>
    /// <returns>A task completing when observation and delivery complete, or faulting on failure.</returns>
    Task OnSendAsync(BehaviorCallContext callContext, Func<Task<SendResult>> next);

    /// <summary>Wraps an awaited request and exposes the unchanged response and status.</summary>
    /// <typeparam name="TResponse">The expected response payload type.</typeparam>
    /// <param name="callContext">The initiating behavior, target, request envelope, and optional containing entity operation.</param>
    /// <param name="next">The remaining observers and request; call and await exactly once to inspect the response and status.</param>
    /// <returns>A task completing when observation and the request complete, or faulting on failure.</returns>
    Task OnRequestAsync<TResponse>(BehaviorCallContext callContext, Func<Task<RequestResult<TResponse>>> next);

    /// <summary>Wraps a logical entity operation, including lookup and cached projection reads.</summary>
    /// <typeparam name="TResult">The result type of the entity API operation.</typeparam>
    /// <param name="operationContext">The operation identity, caller, target definition, and request.</param>
    /// <param name="next">The remaining observers and logical operation; call and await exactly once without substituting its result.</param>
    /// <returns>A task completing when observation and the logical operation complete, or faulting on failure.</returns>
    Task OnEntityOperationAsync<TResult>(EntityOperationContext operationContext, Func<Task<TResult>> next);

    /// <summary>Wraps synchronous dispatch of a fire-and-forget send, not its eventual processing.</summary>
    /// <param name="callContext">The source, target, and envelope; inspect <see cref="BehaviorCallContext.Dispatched"/> after continuation.</param>
    /// <param name="next">The remaining observers and synchronous dispatch; invoke exactly once before returning.</param>
    void OnDispatch(BehaviorCallContext callContext, System.Action next);

}

/// <summary>Identifies a behavior call and its optional containing logical operation.</summary>
/// <param name="Source">The initiating behavior instance.</param>
/// <param name="Target">The intended recipient behavior instance.</param>
/// <param name="Event">The event envelope, including its delivery identity and headers.</param>
/// <param name="ParentOperationId">The containing logical entity operation ID, or null for a standalone call.</param>
public sealed record BehaviorCallContext(
    BehaviorId Source, BehaviorId Target, EventHolder Event, Guid? ParentOperationId)
{
    /// <summary>For a synchronous dispatch, whether a target was found and dispatch was started; otherwise null.</summary>
    public bool? Dispatched { get; internal set; }
}

/// <summary>Identifies an initiating behavior's logical entity operation.</summary>
/// <remarks>
/// DefinitionElementId is the canonical identifier of the requested definition element,
/// derived from the target's registered name and the operation's declared type or field.
/// It does not imply that lookup or execution succeeded. It is null when no owner exists
/// and therefore no target definition can be identified.
/// </remarks>
/// <param name="OperationId">The identity of this logical invocation, correlating nested sends or requests.</param>
/// <param name="Source">The initiating behavior instance.</param>
/// <param name="Owner">The resolved owner instance, or null when no owner exists.</param>
/// <param name="Target">The resolved entity instance, or null when no target can be resolved.</param>
/// <param name="EntityName">The requested registered entity name.</param>
/// <param name="Kind">The requested entity operation category.</param>
/// <param name="Request">The mutation, field request, or projection request payload, when present.</param>
/// <param name="DefinitionElementId">The canonical requested model element identifier, or null without an identifiable owner.</param>
public sealed record EntityOperationContext(
    Guid OperationId, BehaviorId Source, BehaviorId? Owner, BehaviorId? Target,
    string EntityName, EntityOperationKind Kind, object? Request, string? DefinitionElementId);

/// <summary>The kind of logical entity operation.</summary>
public enum EntityOperationKind
{
    /// <summary>Executes a registered mutation.</summary>
    Mutation,
    /// <summary>Writes a field directly using a field-state event.</summary>
    FieldWrite,
    /// <summary>Requests a field's current value.</summary>
    FieldRead,
    /// <summary>Requests a projection value, potentially from a caller-side cache.</summary>
    ProjectionRead
}
