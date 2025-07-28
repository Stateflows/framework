namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IXSubscription<out TReturn>
    {
        TReturn AddSubscription<TNotification>();
    }
}
