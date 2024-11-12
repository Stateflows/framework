namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ISubscription<out TReturn>
    {
        TReturn AddSubscription<TNotificationEvent>();
    }
}
