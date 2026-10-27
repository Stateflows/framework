using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces
{
    /// <summary>Defines persistence for retained notifications associated with behaviors.</summary>
    public interface IStateflowsNotificationsStorage : IDisposable
    {
        /// <summary>Persists notifications for a behavior.</summary>
        /// <param name="behaviorId">The behavior associated with the notifications.</param>
        /// <param name="notifications">The notification envelopes to save.</param>
        Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications);
        /// <summary>Retrieves notifications matching names that were sent after a specified time.</summary>
        /// <param name="behaviorId">The behavior whose notifications are queried.</param>
        /// <param name="notificationNames">The notification names to include.</param>
        /// <param name="lastNotificationCheck">The lower time bound for the query.</param>
        /// <returns>The matching notification envelopes.</returns>
        Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck);
    }
}