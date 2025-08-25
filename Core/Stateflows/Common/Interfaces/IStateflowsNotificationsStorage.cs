using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    public interface IStateflowsNotificationsStorage
    {
        Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications);
        Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck);
    }
}