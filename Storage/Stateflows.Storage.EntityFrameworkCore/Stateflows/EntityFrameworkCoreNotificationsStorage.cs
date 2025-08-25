using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreNotificationsStorage : IStateflowsNotificationsStorage
    {
        private readonly IStateflowsDbContext_v1 DbContext;
        private readonly ILogger<EntityFrameworkCoreNotificationsStorage> Logger;

        public EntityFrameworkCoreNotificationsStorage(IStateflowsDbContext_v1 dbContext, ILogger<EntityFrameworkCoreNotificationsStorage> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }
        
        public Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications)
        {
            DbContext.Notifications_v1.AddRange(notifications.Select(n => new Notification_v1(n)));
            return DbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck)
        {
            var notifications = await DbContext.Notifications_v1.Where(n =>
                n.SenderType == behaviorId.Type &&
                n.SenderName == behaviorId.Name &&
                n.SenderInstance == behaviorId.Instance &&
                notificationNames.Contains(n.Name) &&
                (
                    n.SentAt.AddSeconds(n.TimeToLive) >= lastNotificationCheck ||
                    n.Retained
                )
            ).ToArrayAsync();

            notifications = notifications.Except(notifications.Where(n => 
                n.Retained &&
                notifications.Any(m =>
                    m.Retained &&
                    m.SenderType == n.SenderType &&
                    m.SenderName == n.SenderName &&
                    m.SenderInstance == n.SenderInstance &&
                    m.Name == n.Name &&
                    m.SentAt < n.SentAt
                )
            )).ToArray();

            return notifications.Select(n => (EventHolder)StateflowsJsonConverter.DeserializeObject(n.Data));
        }
    }
}
