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
            
            var retainedNames = notifications.Where(n => n.Retained).Select(n => n.Name).ToArray();
            var now = DateTime.Now;
            var obsolete = await dbContext.Notifications_v1.Where(n =>
                n.SenderType == behaviorId.Type &&
                n.SenderName == behaviorId.Name &&
                n.SenderInstance == behaviorId.Instance &&
                ((!n.Retained && n.TimeToLive <= 0) ||
                    n.Retained ||
                    (n.TimeToLive > 0 && n.SentAt.AddSeconds(n.TimeToLive) <= now)))
                .ToArrayAsync();
            var latestStored = obsolete.Where(n => n.Retained)
                .GroupBy(n => n.Name).Select(group => group.MaxBy(n => n.Id)!).ToHashSet();
            dbContext.Notifications_v1.RemoveRange(obsolete.Where(n =>
                !n.Retained || retainedNames.Contains(n.Name) || !latestStored.Contains(n) ||
                (n.TimeToLive > 0 && n.SentAt.AddSeconds(n.TimeToLive) <= now)));
            var latestRetained = notifications.Where(n => n.Retained)
                .GroupBy(n => n.Name).Select(group => group.Last()).ToHashSet();
            dbContext.Notifications_v1.AddRange(notifications.Where(n =>
                (n.Retained ? latestRetained.Contains(n) : n.TimeToLive > 0) &&
                (n.TimeToLive <= 0 || n.SentAt.AddSeconds(n.TimeToLive) > now))
                .Select(n => new Notification_v1(n)));
            await dbContext.SaveChangesAsync();
                
            dbContext.ChangeTracker.Clear();
        }

        public async Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContextFactory = scope.ServiceProvider.GetService<IDbContextFactory<TDbContext>>() ?? new DbContextFactory<TDbContext>(scope.ServiceProvider);
            var dbContext = await dbContextFactory.CreateDbContextAsync();
            
            var now = DateTime.Now;
            var notifications = await dbContext.Notifications_v1
                .Where(n =>
                    n.SenderType == behaviorId.Type &&
                    n.SenderName == behaviorId.Name &&
                    n.SenderInstance == behaviorId.Instance &&
                    notificationNames.Contains(n.Name) &&
                    (
                        n.SentAt >= lastNotificationCheck ||
                        n.Retained
                    )
                )
                .AsNoTracking()
                .ToArrayAsync();
            
            var latestRetained = notifications.Where(n => n.Retained)
                .GroupBy(n => n.Name)
                .Select(group => group.MaxBy(n => n.Id)!)
                .ToHashSet();
            notifications = notifications.OrderBy(n => n.Id).Where(n =>
                (n.Retained ? latestRetained.Contains(n) : n.TimeToLive > 0) &&
                (n.TimeToLive <= 0 || n.SentAt.AddSeconds(n.TimeToLive) > now) &&
                (n.SentAt >= lastNotificationCheck || latestRetained.Contains(n))).ToArray();
            
            var result = notifications.Select(n => (EventHolder)StateflowsJsonConverter.DeserializeObject(n.Data));
            
            return result;
        }

        public void Dispose()
        { }
    }
}
