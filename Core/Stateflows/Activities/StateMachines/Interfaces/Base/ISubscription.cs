using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface ISubscription<out TReturn>
    {
        TReturn AddSubscription<TNotification>()
            where TNotification : Notification, new();
    }
}
