using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationsHub
    {
        Task PublishAsync(EventHolder eventHolder);

        void RegisterHandler(INotificationHandler notificationHandler);

        void UnregisterHandler(INotificationHandler notificationHandler);

        public Dictionary<BehaviorId, List<EventHolder>> Notifications { get; }
    }
}
