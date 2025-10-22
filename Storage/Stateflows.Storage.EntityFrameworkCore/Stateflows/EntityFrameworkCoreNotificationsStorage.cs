using System.Diagnostics;
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
        internal static readonly ActivitySource Source = new ActivitySource(nameof(Stateflows));
        
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
            var a = Source.StartActivity($"State Machine '{behaviorId.Name.ToShortName()}:{behaviorId.Instance}' running #1 query");
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
            a?.Stop();

            a = Source.StartActivity($"State Machine '{behaviorId.Name.ToShortName()}:{behaviorId.Instance}' running #2 query");
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
            a?.Stop();

            a = Source.StartActivity($"State Machine '{behaviorId.Name.ToShortName()}:{behaviorId.Instance}' running serialization");
            var result = notifications.Select(n => (EventHolder)StateflowsJsonConverter.DeserializeObject(n.Data));
            a?.Stop();
            
            return result;
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
