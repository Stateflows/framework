using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Observes state-machine execution operations without changing their results.</summary>
    /// <remarks>
    /// Global observers wrap local observers; observers within each group run in registration order.
    /// Each observer must invoke and await its <c>next</c> continuation exactly once. The result
    /// produced by a continuation is exposed for observation only and cannot be replaced. If an
    /// observer throws, its continuation is drained before the failure propagates; catching a
    /// continuation failure does not suppress it. Use <see langword="finally"/> for cleanup that
    /// must run on either success or failure. Missing, repeated, or unfinished continuation calls
    /// fail execution.
    /// See <see href="https://github.com/Stateflows/framework/wiki/4.5.-Observability">observability</see>.
    /// </remarks>
    public interface IStateMachineObserver
    {
        /// <summary>Observes one registered action while it executes within its aggregate operation.</summary>
        /// <param name="actionContext">The identity, owner, operation kind, and event of the action being observed.</param>
        /// <param name="next">The action continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after the continuation and observation complete; it does not carry or replace an action result.</returns>
        Task OnActionAsync(IStateMachineActionObservationContext actionContext, Func<Task> next);

        /// <summary>Observes an individual initialization, transition-guard, or deferral-guard predicate or Boolean group.</summary>
        /// <remarks>
        /// Runs only for registrations actually evaluated, preserving initializer and Boolean-group
        /// short-circuit order. Whole-operation initialization, transition-guard, and deferral-guard
        /// callbacks surround these individual evaluations.
        /// </remarks>
        /// <param name="predicateContext">The predicate registration identity, owning model element, Boolean kind, and current event.</param>
        /// <param name="next">The remaining observers and predicate evaluation; invoke and await exactly once without replacing its result.</param>
        /// <returns>A task completed after evaluation and observation; failures propagate unchanged.</returns>
        Task OnPredicateAsync(IStateMachinePredicateObservationContext predicateContext, Func<Task<bool>> next);

        /// <summary>Observes scheduling initialization of a state's embedded behavior.</summary>
        /// <param name="hostedBehaviorContext">The hosted relationship, owning state, destination instance, and delivery being scheduled.</param>
        /// <param name="next">The scheduling continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after scheduling and observation, not after the embedded behavior finishes initialization.</returns>
        Task OnHostedBehaviorInitializeAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next);

        /// <summary>Observes scheduling finalization of a state's embedded behavior.</summary>
        /// <param name="hostedBehaviorContext">The hosted relationship, owning state, destination instance, and delivery being scheduled.</param>
        /// <param name="next">The scheduling continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after scheduling and observation, not after the embedded behavior finishes finalization.</returns>
        Task OnHostedBehaviorFinalizeAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next);

        /// <summary>Observes forwarding an event to a state's embedded behavior.</summary>
        /// <param name="hostedBehaviorContext">The hosted relationship, owning state, destination instance, and forwarded delivery.</param>
        /// <param name="next">The forwarding continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after forwarding and observation; it does not imply completion of the embedded behavior's event processing.</returns>
        Task OnHostedBehaviorEventForwardAsync(IHostedBehaviorObservationContext hostedBehaviorContext, Func<Task> next);

        /// <summary>Observes evaluation of a candidate event against a state's deferral guard.</summary>
        /// <remarks>
        /// Surrounds the rule's full predicate pipeline, including typed activation and individual
        /// <see cref="OnPredicateAsync"/> callbacks. During reevaluation the context and execution
        /// event describe the queued candidate. Guard and observer failures use
        /// <see cref="IStateMachineExceptionHandler.OnDeferralGuardExceptionAsync"/>.
        /// Enqueueing is a separate operation observed by <see cref="OnEventDeferredAsync"/>.
        /// </remarks>
        /// <param name="deferralContext">The matching deferral rule, owning state, and candidate event; it indicates whether this is reevaluation.</param>
        /// <param name="next">The guard continuation; invoke and await it exactly once to evaluate the candidate.</param>
        /// <returns>A task that completes after guard evaluation and observation; the continuation's Boolean result cannot be replaced.</returns>
        Task OnDeferralGuardAsync(IDeferralObservationContext deferralContext, Func<Task<bool>> next);

        /// <summary>Observes adding a candidate event to the deferred queue.</summary>
        /// <remarks>This callback runs before transaction commit; observing a successful continuation does not by itself mean the transaction has committed.</remarks>
        /// <param name="deferralContext">The deferral rule, owning state, and event being queued.</param>
        /// <param name="next">The queueing continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after the queueing operation and observation complete.</returns>
        Task OnEventDeferredAsync(IDeferralObservationContext deferralContext, Func<Task> next);

        /// <summary>Observes an attempt to initialize the state machine.</summary>
        /// <remarks>
        /// Wraps the whole initialization attempt, including any individual initializer
        /// <see cref="OnPredicateAsync"/> callbacks. The continuation exposes the initialization
        /// acceptance result; observing it does not permit replacing that result.
        /// </remarks>
        /// <param name="initializationContext">The execution context of the machine being initialized.</param>
        /// <param name="implicitInitialization"><see langword="true"/> when an ordinary event caused initialization; otherwise initialization was explicitly requested.</param>
        /// <param name="next">The initialization continuation; invoke and await it exactly once to obtain the acceptance result.</param>
        /// <returns>A task that completes after initialization and observation; the continuation's acceptance result cannot be replaced.</returns>
        Task OnStateMachineInitializeAsync(IStateMachineInitializationContext initializationContext, bool implicitInitialization, Func<Task<bool>> next);

        /// <summary>Observes machine finalization after a final configuration is reached or finalization is requested.</summary>
        /// <param name="finalizationContext">The machine action context for the finalization operation.</param>
        /// <param name="next">The finalization continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after finalization and observation complete.</returns>
        Task OnStateMachineFinalizeAsync(IStateMachineActionContext finalizationContext, Func<Task> next);

        /// <summary>Observes initialization of a composite or orthogonal state's nested configuration.</summary>
        /// <param name="stateContext">The action context for the state whose nested configuration is being initialized.</param>
        /// <param name="next">The nested-state initialization continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after the state operation and observation complete.</returns>
        Task OnStateInitializeAsync(IStateActionContext stateContext, Func<Task> next);

        /// <summary>Observes finalization of a composite or orthogonal state's nested configuration.</summary>
        /// <param name="stateContext">The action context for the state whose nested configuration is being finalized.</param>
        /// <param name="next">The nested-state finalization continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after the state operation and observation complete.</returns>
        Task OnStateFinalizeAsync(IStateActionContext stateContext, Func<Task> next);

        /// <summary>Observes entry into a state, including its registered entry actions.</summary>
        /// <param name="stateContext">The action context for the state being entered.</param>
        /// <param name="next">The state-entry continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after state entry and observation complete.</returns>
        Task OnStateEntryAsync(IStateActionContext stateContext, Func<Task> next);

        /// <summary>Observes exit from a state, including its registered exit actions.</summary>
        /// <param name="stateContext">The action context for the state being exited.</param>
        /// <param name="next">The state-exit continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after state exit and observation complete.</returns>
        Task OnStateExitAsync(IStateActionContext stateContext, Func<Task> next);

        /// <summary>Observes eligibility evaluation of a candidate transition without changing its guard result.</summary>
        /// <remarks>Wraps the transition's whole guard attempt; <see cref="OnPredicateAsync"/> observes only predicates and Boolean groups actually evaluated inside it.</remarks>
        /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
        /// <param name="transitionContext">The candidate transition and its triggering event.</param>
        /// <param name="next">The guard continuation; invoke and await it exactly once to obtain the eligibility result.</param>
        /// <returns>A task that completes after guard evaluation and observation; the continuation's Boolean result cannot be replaced.</returns>
        Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Func<Task<bool>> next);

        /// <summary>Observes the effect executed for a selected transition.</summary>
        /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
        /// <param name="transitionContext">The selected transition and its triggering event.</param>
        /// <param name="next">The transition-effect continuation; invoke and await it exactly once.</param>
        /// <returns>A task that completes after the effect and observation complete.</returns>
        Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Func<Task> next);
    }
}
