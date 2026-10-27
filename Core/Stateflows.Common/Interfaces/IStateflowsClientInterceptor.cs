namespace Stateflows.Common
{
    /// <summary>Intercepts event dispatch initiated by a Stateflows client.</summary>
    public interface IStateflowsClientInterceptor
    {
        /// <summary>Runs before an event is dispatched to its target behavior.</summary>
        /// <param name="eventHolder">The event envelope being dispatched.</param>
        /// <returns><see langword="true"/> to continue dispatch; <see langword="false"/> to cancel it.</returns>
        bool BeforeDispatchEvent(EventHolder eventHolder);
        /// <summary>Runs after an event dispatch attempt completes.</summary>
        /// <param name="eventHolder">The event envelope that was dispatched.</param>
        void AfterDispatchEvent(EventHolder eventHolder);
    }
}
