namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IStateSubscription<out TReturn>
    {
        TReturn AddSubscription<TNotificationEvent>();
    }
}
