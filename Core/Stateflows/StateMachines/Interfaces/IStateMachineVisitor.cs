using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Receives callbacks describing a finalized state-machine definition.</summary>
/// <remarks>
/// Callbacks are awaited serially in declaration/model traversal order. The machine identity
/// (<c>stateMachineId</c>, <c>stateMachineName</c>, <c>stateMachineVersion</c>) is repeated first in every callback; element identifiers and
/// related element identifiers identify definition elements, not runtime machine instances.
/// A referenced element is reported before its implementation-type or nested metadata callbacks.
/// <see cref="StateMachineAddedAsync"/> is always the final callback for a definition.
/// </remarks>
public interface IStateMachineVisitor
{
    /// <summary>Begins reporting a finalized state-machine definition.</summary>
    /// <param name="stateMachineId">The definition-level identifier of this state machine.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="ownerClass">The owning behavior class, or <see langword="null"/> when there is no owner.</param>
    /// <param name="parentClass">The parent behavior class, or <see langword="null"/> when there is no parent.</param>
    /// <param name="hasDefaultInstance">Whether a default instance is configured for this behavior class.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before proceeding.</returns>
    Task StateMachineAddingAsync(string stateMachineId, string stateMachineName, int stateMachineVersion, BehaviorClass? ownerClass, BehaviorClass? parentClass, bool hasDefaultInstance);

    /// <summary>Reports the CLR state-machine implementation type, when one is registered.</summary>
    /// <typeparam name="TStateMachine">The concrete state-machine implementation type.</typeparam>
    /// <param name="stateMachineId">The definition-level identifier of this state machine.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before proceeding.</returns>
    Task StateMachineTypeAddedAsync<TStateMachine>(string stateMachineId, string stateMachineName, int stateMachineVersion) where TStateMachine : class, IStateMachine;

    /// <summary>Completes reporting the definition after all its elements and metadata have been visited.</summary>
    /// <param name="stateMachineId">The definition-level identifier of this state machine.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; this is the last callback in the visitation.</returns>
    Task StateMachineAddedAsync(string stateMachineId, string stateMachineName, int stateMachineVersion);

    /// <summary>Reports a vertex (state or pseudostate) before its implementation type and nested regions.</summary>
    /// <param name="vertexId">The definition-level identifier of the vertex.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="vertexName">The vertex's registered name.</param>
    /// <param name="vertexType">The semantic kind of vertex.</param>
    /// <param name="parentVertexId">The containing vertex identifier, or <see langword="null"/> if this vertex belongs to the machine's root region.</param>
    /// <param name="regionId">The containing region identifier, or <see langword="null"/> if there is no containing region.</param>
    /// <param name="regionIndex">The zero-based position of the containing region in its parent vertex, or <see langword="null"/> when there is no parent vertex.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before proceeding.</returns>
    Task VertexAddedAsync(string vertexId, string stateMachineName, int stateMachineVersion, string vertexName, VertexType vertexType, string? parentVertexId, string? regionId, int? regionIndex);

    /// <summary>Reports the CLR implementation type for a vertex, when one is registered.</summary>
    /// <typeparam name="TVertex">The concrete vertex implementation type.</typeparam>
    /// <param name="vertexId">The definition-level identifier of the vertex described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="VertexAddedAsync"/> callback.</returns>
    Task VertexTypeAddedAsync<TVertex>(string vertexId, string stateMachineName, int stateMachineVersion) where TVertex : class, IVertex;

    /// <summary>Reports a nested region before visiting its vertices.</summary>
    /// <param name="regionId">The definition-level identifier of the region.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="parentVertexId">The identifier of the vertex that owns the region.</param>
    /// <param name="regionIndex">The zero-based position of the region in its parent vertex.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before visiting the region's vertices.</returns>
    Task RegionAddedAsync(string regionId, string stateMachineName, int stateMachineVersion, string parentVertexId, int regionIndex);

    /// <summary>Reports a transition before its implementation type, guards, and effects.</summary>
    /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
    /// <param name="transitionId">The definition-level identifier of the transition.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="sourceVertexId">The identifier of the transition's source vertex.</param>
    /// <param name="targetVertexId">The identifier of its target vertex, or <see langword="null"/> for a transition with no explicit target.</param>
    /// <param name="isElse">Whether this is an else transition, considered after other eligible transitions from its source.</param>
    /// <param name="isLocal">Whether the transition is local rather than external.</param>
    /// <param name="polymorphicTriggers">Whether trigger matching includes derived event payload types.</param>
    /// <param name="triggerNames">The event names that trigger this transition, in ordinal-sorted order.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before proceeding.</returns>
    Task TransitionAddedAsync<TEvent>(string transitionId, string stateMachineName, int stateMachineVersion, string sourceVertexId, string? targetVertexId, bool isElse, bool isLocal, bool polymorphicTriggers, IReadOnlyList<string> triggerNames);

    /// <summary>Reports the CLR implementation type for a transition, when one is registered.</summary>
    /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
    /// <typeparam name="TTransition">The concrete transition implementation type.</typeparam>
    /// <param name="transitionId">The definition-level identifier of the transition described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="TransitionAddedAsync{TEvent}"/> callback.</returns>
    Task TransitionTypeAddedAsync<TEvent, TTransition>(string transitionId, string stateMachineName, int stateMachineVersion) where TTransition : class, ITransition<TEvent>;

    /// <summary>Reports an event-specific machine initializer before its guard registrations.</summary>
    /// <typeparam name="TEvent">The CLR payload type associated with this initializer.</typeparam>
    /// <param name="initializerId">The definition-level identifier of the initializer.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="eventNames">The event names mapped to this initializer, in ordinal-sorted order.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before reporting the initializer's guards.</returns>
    Task InitializerAddedAsync<TEvent>(string initializerId, string stateMachineName, int stateMachineVersion, IReadOnlyList<string> eventNames);

    /// <summary>Reports the default initializer before its guard registrations, when it exists.</summary>
    /// <param name="initializerId">The definition-level identifier of the default initializer.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before reporting the initializer's guards.</returns>
    Task DefaultInitializerAddedAsync(string initializerId, string stateMachineName, int stateMachineVersion);

    /// <summary>Reports the machine finalizer before its action registrations, when it exists.</summary>
    /// <param name="finalizerId">The definition-level identifier of the machine finalizer.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before reporting the finalizer's actions.</returns>
    Task FinalizerAddedAsync(string finalizerId, string stateMachineName, int stateMachineVersion);

    /// <summary>Reports an action registration in its owning state or transition operation.</summary>
    /// <param name="actionId">The definition-level identifier of the action registration.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="ownerElementId">The identifier of the state, transition, initializer, finalizer, or other element that owns the action.</param>
    /// <param name="operationKind">The operation to which the action belongs.</param>
    /// <param name="actionIndex">The zero-based position of the action within its operation.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before any action type or behavior metadata.</returns>
    Task ActionAddedAsync(string actionId, string stateMachineName, int stateMachineVersion, string ownerElementId, StateMachineOperation operationKind, int actionIndex);

    /// <summary>Reports the CLR implementation type for an action registration, when one is registered.</summary>
    /// <typeparam name="TImplementation">The concrete action implementation type.</typeparam>
    /// <param name="actionId">The definition-level identifier of the action registration described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="ActionAddedAsync"/> callback.</returns>
    Task ActionTypeAddedAsync<TImplementation>(string actionId, string stateMachineName, int stateMachineVersion) where TImplementation : class;

    /// <summary>Reports the behavior invoked by an action registration, when the action delegates to a behavior.</summary>
    /// <param name="actionId">The definition-level identifier of the action registration described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="actionBehaviorClass">The behavior class invoked by the action.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="ActionAddedAsync"/> callback and any action type callback.</returns>
    Task ActionBehaviorAddedAsync(string actionId, string stateMachineName, int stateMachineVersion, BehaviorClass actionBehaviorClass);

    /// <summary>Reports a Boolean guard registration before its type, behavior, and child-guard metadata.</summary>
    /// <param name="guardId">The definition-level identifier of the guard registration.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="ownerElementId">The identifier of the state, transition, deferral, or initializer that owns the guard tree.</param>
    /// <param name="parentGuardId">The parent guard identifier, or <see langword="null"/> for a root guard.</param>
    /// <param name="guardIndex">The zero-based position of this guard among its siblings.</param>
    /// <param name="guardKind">The Boolean operation or predicate represented by this guard registration.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before reporting child guards.</returns>
    Task GuardAddedAsync(string guardId, string stateMachineName, int stateMachineVersion, string ownerElementId, string? parentGuardId, int guardIndex, GuardKind guardKind);

    /// <summary>Reports the CLR implementation type for a guard registration, when one is registered.</summary>
    /// <typeparam name="TImplementation">The concrete guard implementation type.</typeparam>
    /// <param name="guardId">The definition-level identifier of the guard registration described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="GuardAddedAsync"/> callback.</returns>
    Task GuardTypeAddedAsync<TImplementation>(string guardId, string stateMachineName, int stateMachineVersion) where TImplementation : class;

    /// <summary>Reports the behavior invoked by a guard registration, when the guard delegates to a behavior.</summary>
    /// <param name="guardId">The definition-level identifier of the guard registration described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="guardBehaviorClass">The behavior class invoked by the guard.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="GuardAddedAsync"/> callback and any guard type callback.</returns>
    Task GuardBehaviorAddedAsync(string guardId, string stateMachineName, int stateMachineVersion, BehaviorClass guardBehaviorClass);

    /// <summary>Reports a grouped event-deferral rule before its guard registrations.</summary>
    /// <typeparam name="TEvent">The CLR payload type associated with the deferral rule.</typeparam>
    /// <param name="deferralId">The definition-level identifier of the deferral rule.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="vertexId">The identifier of the state that owns the rule.</param>
    /// <param name="eventNames">The event names handled by the rule, in ordinal-sorted order.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before reporting the rule's guards.</returns>
    Task DeferralAddedAsync<TEvent>(string deferralId, string stateMachineName, int stateMachineVersion, string vertexId, IReadOnlyList<string> eventNames);

    /// <summary>Reports a behavior hosted by a state before its implementation type and event bindings.</summary>
    /// <param name="hostedBehaviorId">The definition-level identifier of the hosted-behavior relationship.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="vertexId">The identifier of the state that hosts the behavior.</param>
    /// <param name="hostedBehaviorClass">The behavior class hosted by the state.</param>
    /// <returns>A task that completes when this callback has finished; visitation awaits it before hosted-behavior metadata.</returns>
    Task HostedBehaviorAddedAsync(string hostedBehaviorId, string stateMachineName, int stateMachineVersion, string vertexId, BehaviorClass hostedBehaviorClass);

    /// <summary>Reports the CLR implementation type for a hosted behavior, when one is registered.</summary>
    /// <typeparam name="TImplementation">The concrete hosted-behavior implementation type.</typeparam>
    /// <param name="hostedBehaviorId">The definition-level identifier of the hosted-behavior relationship described by this metadata.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="HostedBehaviorAddedAsync"/> callback.</returns>
    Task HostedBehaviorTypeAddedAsync<TImplementation>(string hostedBehaviorId, string stateMachineName, int stateMachineVersion) where TImplementation : class;

    /// <summary>Reports an event binding that forwards an event to a hosted behavior.</summary>
    /// <typeparam name="TEvent">The CLR payload type of the bound event.</typeparam>
    /// <param name="hostedBehaviorEventId">The definition-level identifier of this hosted-behavior event binding.</param>
    /// <param name="stateMachineName">The registered state-machine behavior name.</param>
    /// <param name="stateMachineVersion">The registered definition version.</param>
    /// <param name="hostedBehaviorId">The identifier of the hosted-behavior relationship receiving the event.</param>
    /// <param name="eventNames">The event names forwarded by this binding, in ordinal-sorted order.</param>
    /// <returns>A task that completes when this callback has finished; it follows the corresponding <see cref="HostedBehaviorAddedAsync"/> callback.</returns>
    Task HostedBehaviorEventAddedAsync<TEvent>(string hostedBehaviorEventId, string stateMachineName, int stateMachineVersion, string hostedBehaviorId, IReadOnlyList<string> eventNames);
}
