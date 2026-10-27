#nullable enable

namespace Stateflows.Testing.StateMachines.Sequence
{
    /// <summary>Builds an expected ordered sequence of state-machine lifecycle and transition callbacks.</summary>
    /// <remarks>Verification treats the expected steps as an ordered subsequence: additional runtime callbacks between expected steps are allowed.</remarks>
    public interface IExecutionSequenceBuilder
    {
        /// <summary>Expects state-machine initialization.</summary>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateMachineInitialize();
        /// <summary>Expects state-machine finalization.</summary>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateMachineFinalize();

        /// <summary>Expects initialization of the named state.</summary>
        /// <param name="stateName">The registered state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateInitialize(string stateName);
        /// <summary>Expects finalization of the named state.</summary>
        /// <param name="stateName">The registered state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateFinalize(string stateName);
        /// <summary>Expects entry into the named state.</summary>
        /// <param name="stateName">The registered state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateEntry(string stateName);
        /// <summary>Expects exit from the named state.</summary>
        /// <param name="stateName">The registered state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder StateExit(string stateName);

        /// <summary>Expects the guard for an event-triggered transition to be evaluated.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="sourceStateName">The transition's source state name.</param>
        /// <param name="targetStateName">The target state name, or <see langword="null"/> for an internal transition.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder TransitionGuard(string eventName, string sourceStateName, string? targetStateName);
        /// <summary>Expects the effect for an event-triggered transition to be executed.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="sourceStateName">The transition's source state name.</param>
        /// <param name="targetStateName">The target state name, or <see langword="null"/> for an internal transition.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder TransitionEffect(string eventName, string sourceStateName, string? targetStateName);

        /// <summary>Expects the guard for an internal event-triggered transition to be evaluated.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="sourceStateName">The state containing the internal transition.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder InternalTransitionGuard(string eventName, string sourceStateName);
        /// <summary>Expects the effect for an internal event-triggered transition to be executed.</summary>
        /// <param name="eventName">The registered event name.</param>
        /// <param name="sourceStateName">The state containing the internal transition.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder InternalTransitionEffect(string eventName, string sourceStateName);

        /// <summary>Expects the guard on the default completion transition between two states.</summary>
        /// <param name="sourceStateName">The transition's source state name.</param>
        /// <param name="targetStateName">The transition's target state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder DefaultTransitionGuard(string sourceStateName, string targetStateName);
        /// <summary>Expects the effect on the default completion transition between two states.</summary>
        /// <param name="sourceStateName">The transition's source state name.</param>
        /// <param name="targetStateName">The transition's target state name.</param>
        /// <returns>This sequence builder for chaining expected steps.</returns>
        IExecutionSequenceBuilder DefaultTransitionEffect(string sourceStateName, string targetStateName);
    }
}
