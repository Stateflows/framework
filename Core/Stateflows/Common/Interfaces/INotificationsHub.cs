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
        
        Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime? lastNotificationCheck = null);
        
        Task<IEnumerable<TNotification>> GetNotificationsAsync<TNotification>(BehaviorId behaviorId, DateTime? lastNotificationCheck = null);
    }
}
