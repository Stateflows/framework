using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationsHub
    {
        Task PublishAsync<TNotification>(TNotification notification)
            where TNotification : Notification, new();
        
        event Action<Notification> OnPublish;

        public Dictionary<BehaviorId, List<Notification>> Notifications { get; }
    }
}
