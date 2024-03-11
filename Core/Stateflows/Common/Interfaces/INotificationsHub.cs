using System;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationsHub
    {
        Notification[] GetPendingNotifications(BehaviorId behaviorId, DateTime notificationThreshold);
    }
}
