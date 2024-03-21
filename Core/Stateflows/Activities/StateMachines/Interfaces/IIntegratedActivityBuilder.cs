using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    public interface IIntegratedActivityBuilder
    {
        IIntegratedActivityBuilder AddSubscription<TNotification>()
            where TNotification : Notification, new();
    }
}
