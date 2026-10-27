using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>
    /// Acknowledges state-machine failures without recovering execution. All handlers are awaited sequentially;
    /// returning true acknowledges the exception. Execution still fails and its context
    /// changes are not persisted. IDE break-on-thrown settings control debugger stops
    /// independently of acknowledgement.
    /// See <see href="https://github.com/Stateflows/framework/wiki/observability">observability</see>.
    /// </summary>
    public interface IStateMachineExceptionHandler
    {
        /// <summary>Receives a failure from a machine initialization attempt.</summary>
        /// <param name="context">The initialization context at the failure.</param>
        /// <param name="exception">The exception raised by initialization.</param>
        /// <returns>Whether this handler acknowledges the failure; acknowledgement does not resume execution or commit changes.</returns>
        Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception);

        /// <summary>Receives a failure from machine finalization.</summary>
        /// <param name="context">The machine context at the failure.</param>
        /// <param name="exception">The exception raised by finalization.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception);

        /// <summary>Receives a failure while evaluating a transition guard.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="context">The candidate transition's context.</param>
        /// <param name="exception">The guard failure.</param>
        /// <returns>Whether this handler acknowledges the failure; this is not a replacement guard result.</returns>
        Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        /// <summary>Receives a failure from a selected transition's effect.</summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="context">The executing transition's context.</param>
        /// <param name="exception">The effect failure.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception);

        /// <summary>Receives a failure while initializing a composite or orthogonal state.</summary>
        /// <param name="context">The initializing state's context.</param>
        /// <param name="exception">The initialization failure.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception);

        /// <summary>Receives a failure while finalizing a composite or orthogonal state.</summary>
        /// <param name="context">The finalizing state's context.</param>
        /// <param name="exception">The finalization failure.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception);

        /// <summary>Receives a failure while executing state entry actions.</summary>
        /// <param name="context">The entered state's context.</param>
        /// <param name="exception">The entry failure.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception);

        /// <summary>Receives a failure while executing state exit actions.</summary>
        /// <param name="context">The exited state's context.</param>
        /// <param name="exception">The exit failure.</param>
        /// <returns>Whether this handler acknowledges the failure, without recovering execution.</returns>
        Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception);
    }
}
