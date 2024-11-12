using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stateflows.Common.Interfaces
{
    public interface INotificationsHub
    {
        Task PublishAsync(EventHolder eventHolder);
        
        event Action<EventHolder> OnPublish;

        public Dictionary<BehaviorId, List<EventHolder>> Notifications { get; }
    }
}
