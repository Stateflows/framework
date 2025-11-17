namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateUtils<out TReturn>
    {
        /// <summary>
        /// Adds a <a href="https://github.com/Stateflows/framework/wiki/States#Deferred-Events">deferred event</a> to the current state.<br/><br/>
        /// When a state that defers event is active, a deferred event is not processed when received.
        /// Instead, it is stored to be handled later, as soon as state machine reaches the configuration which does not defer this event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to be deferred.</typeparam>
        TReturn AddDeferredEvent<TEvent>(DeferralBuildAction<TEvent> buildAction = null);
    }
}
