using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Storage
{
    public class InMemoryNotificationsStorage : IStateflowsNotificationsStorage
    {
        private readonly Dictionary<string, Dictionary<BehaviorId, List<EventHolder>>> Notifications = new Dictionary<string, Dictionary<BehaviorId, List<EventHolder>>>();
        
        public InMemoryNotificationsStorage(ITenantAccessor tenantAccessor)
        {
            TenantAccessor = tenantAccessor;
        }

        private readonly ITenantAccessor TenantAccessor;
        
        public Task SaveNotificationsAsync(BehaviorId behaviorId, EventHolder[] notifications)
        {
            lock (Notifications)
            {
                if (!Notifications.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantNotifications))
                {
                    tenantNotifications = new Dictionary<BehaviorId, List<EventHolder>>();
                    Notifications.Add(TenantAccessor.CurrentTenantId, tenantNotifications);
                }

                if (!tenantNotifications.TryGetValue(behaviorId, out var behaviorNotifications))
                {
                    behaviorNotifications = [];
                    tenantNotifications.Add(behaviorId, behaviorNotifications);
                }
                
                behaviorNotifications.AddRange(notifications.Where(n => n.Retained || n.TimeToLive > 0));
                Prune(behaviorNotifications);
            }
            
            return Task.CompletedTask;
        }

        public Task<IEnumerable<EventHolder>> GetNotificationsAsync(BehaviorId behaviorId, string[] notificationNames, DateTime lastNotificationCheck)
        {
            lock (Notifications)
            {
                if (!Notifications.TryGetValue(TenantAccessor.CurrentTenantId, out var tenantNotifications))
                {
                    tenantNotifications = new Dictionary<BehaviorId, List<EventHolder>>();
                    Notifications.Add(TenantAccessor.CurrentTenantId, tenantNotifications);
                }

                if (tenantNotifications.TryGetValue(behaviorId, out var behaviorNotifications))
                {
                    Prune(behaviorNotifications);
                    var now = DateTime.Now;
                    var matching = behaviorNotifications.Where(n => notificationNames.Contains(n.Name)).ToArray();
                    var latestRetained = matching.Where(n => n.Retained)
                        .GroupBy(n => n.Name)
                        .Select(group => group.Last())
                        .ToHashSet();
                    return Task.FromResult<IEnumerable<EventHolder>>(matching
                        .Where(n => (n.TimeToLive == 0 || n.SentAt.AddSeconds(n.TimeToLive) > now) &&
                            (n.SentAt >= lastNotificationCheck || latestRetained.Contains(n)))
                        .ToArray());
                }
            }

            return Task.FromResult<IEnumerable<EventHolder>>(Array.Empty<EventHolder>());
        }

        private static void Prune(List<EventHolder> notifications)
        {
            var now = DateTime.Now;
            var latestRetained = notifications.Where(n => n.Retained)
                .GroupBy(n => n.Name).Select(group => group.Last()).ToHashSet();
            notifications.RemoveAll(n =>
                (!n.Retained && n.TimeToLive <= 0) ||
                (n.Retained && !latestRetained.Contains(n)) ||
                (n.TimeToLive > 0 && n.SentAt.AddSeconds(n.TimeToLive) <= now));
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}