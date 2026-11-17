using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Receives failures from state-machine operations and reports whether each failure is acknowledged.</summary>
    /// <remarks>
    /// All registered handlers are awaited sequentially, even after one returns <see langword="true"/>.
    /// Acknowledgement does not resume or recover the failed operation: execution still fails and
    /// its context changes are not persisted. An acknowledged failure is not forwarded to plugins.
    /// An exception thrown by a handler is recorded and logged without preventing later handlers
    /// from running. Debugger break-on-thrown settings control debugger stops independently of
    /// acknowledgement. Direct implementors must implement every callback, including
    /// <see cref="OnDeferralGuardExceptionAsync(IDeferralObservationContext, Exception)"/>; subclasses
    /// of <see cref="StateMachineExceptionHandler"/> inherit its default unacknowledged implementation.
    /// See <see href="https://github.com/Stateflows/framework/wiki/4.5.-Observability">observability</see>.
    /// </remarks>
    public interface IStateMachineExceptionHandler
    {
        /// <summary>Receives an exception raised while attempting to initialize the machine.</summary>
        /// <param name="initializationContext">The machine initialization context at the failure.</param>
        /// <param name="exception">The exception raised by the initialization attempt.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext initializationContext, Exception exception);

        /// <summary>Receives an exception raised while finalizing the machine.</summary>
        /// <param name="finalizationContext">The machine action context at the failure.</param>
        /// <param name="exception">The exception raised by machine finalization.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext finalizationContext, Exception exception);

        /// <summary>Receives an exception raised while evaluating a candidate event's deferral guard.</summary>
        /// <param name="context">The state and candidate event whose deferral guard failed. During reevaluation this describes the deferred candidate, not the event that triggered reevaluation.</param>
        /// <param name="exception">The exception raised during deferral guard evaluation.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnDeferralGuardExceptionAsync(IDeferralObservationContext context, Exception exception);

        /// <summary>Receives an exception raised while evaluating a candidate transition's guard.</summary>
        /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
        /// <param name="transitionContext">The candidate transition and its triggering event at the failure.</param>
        /// <param name="exception">The exception raised during guard evaluation.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; this is not a replacement guard result and does not recover execution.</returns>
        Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Exception exception);

        /// <summary>Receives an exception raised while executing the effect of a selected transition.</summary>
        /// <typeparam name="TEvent">The CLR payload type of the transition's triggering event.</typeparam>
        /// <param name="transitionContext">The selected transition and its triggering event at the failure.</param>
        /// <param name="exception">The exception raised by the transition effect.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> transitionContext, Exception exception);

        /// <summary>Receives an exception raised while initializing a composite or orthogonal state's nested configuration.</summary>
        /// <param name="stateContext">The action context for the state whose nested configuration failed to initialize.</param>
        /// <param name="exception">The exception raised during state initialization.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateInitializationExceptionAsync(IStateActionContext stateContext, Exception exception);

        /// <summary>Receives an exception raised while finalizing a composite or orthogonal state's nested configuration.</summary>
        /// <param name="stateContext">The action context for the state whose nested configuration failed to finalize.</param>
        /// <param name="exception">The exception raised during state finalization.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext stateContext, Exception exception);

        /// <summary>Receives an exception raised while executing a state's entry actions.</summary>
        /// <param name="stateContext">The action context for the state whose entry failed.</param>
        /// <param name="exception">The exception raised during state entry.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateEntryExceptionAsync(IStateActionContext stateContext, Exception exception);

        /// <summary>Receives an exception raised while executing a state's exit actions.</summary>
        /// <param name="stateContext">The action context for the state whose exit failed.</param>
        /// <param name="exception">The exception raised during state exit.</param>
        /// <returns>A task producing whether this handler acknowledges the failure; acknowledgement does not recover execution.</returns>
        Task<bool> OnStateExitExceptionAsync(IStateActionContext stateContext, Exception exception);
    }
}
