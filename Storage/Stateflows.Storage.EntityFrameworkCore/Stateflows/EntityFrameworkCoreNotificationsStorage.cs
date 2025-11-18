using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore;
using Stateflows.Storage.EntityFrameworkCore.EntityFrameworkCore.Entities;

namespace Stateflows.Storage.EntityFrameworkCore.Stateflows
{
    internal class EntityFrameworkCoreNotificationsStorage<TDbContext>(IServiceProvider serviceProvider) : IStateflowsNotificationsStorage
        where TDbContext : DbContext, IStateflowsDbContext_v1
    {
        public async Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            dbContext.Notifications_v1.AddRange(notifications.Select(n => new Notification_v1(n)));
            await dbContext.SaveChangesAsync();
                
            dbContext.ChangeTracker.Clear();
        }

        public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var notifications = await dbContext.Notifications_v1
                .Where(n =>
                    n.SenderType == behaviorId.Type &&
                    n.SenderName == behaviorId.Name &&
                    n.SenderInstance == behaviorId.Instance &&
                    notificationNames.Contains(n.Name) &&
                    (
                        n.SentAt.AddSeconds(n.TimeToLive) >= lastNotificationCheck ||
                        n.Retained
                    )
                )
                .AsNoTracking()
                .ToArrayAsync();
            
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
            
            var result = notifications.Select(n => (EventHolder)StateflowsJsonConverter.DeserializeObject(n.Data));
            
            return result;
        }

        public void Dispose()
        { }
    }
}
