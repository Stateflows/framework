using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationsHub
    {
        Task PublishAsync(EventHolder eventHolder);
        Task PublishRangeAsync(IEnumerable<EventHolder> eventHolders);

        void RegisterHandler(INotificationHandler notificationHandler);

        void UnregisterHandler(INotificationHandler notificationHandler);

        // object LockObject { get; }
        // Dictionary<BehaviorId, List<EventHolder>> GetNotifications();
        Task<EventHolder[]> GetNotificationsAsync(BehaviorId behaviorId, Func<EventHolder, bool> filter = null);
    }
}
