namespace Stateflows.Common
{
    /// <summary>Intercepts execution of an event by a hosted behavior.</summary>
    public interface IStateflowsExecutionInterceptor
    {
        /// <summary>Runs immediately before a behavior begins processing an event.</summary>
        /// <param name="behaviorId">The identifier of the behavior instance.</param>
        /// <param name="eventHolder">The event envelope about to be processed.</param>
        /// <returns><see langword="true"/> to proceed; <see langword="false"/> to cancel processing.</returns>
        bool BeforeExecute(BehaviorId behaviorId, EventHolder eventHolder);
        /// <summary>Runs after the behavior's event processing attempt completes.</summary>
        /// <param name="behaviorId">The identifier of the behavior instance.</param>
        /// <param name="eventHolder">The event envelope that was processed.</param>
        void AfterExecute(BehaviorId behaviorId, EventHolder eventHolder);
    }
}
