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
                    behaviorNotifications = new List<EventHolder>();
                    tenantNotifications.Add(behaviorId, behaviorNotifications);
                }
                
                behaviorNotifications.AddRange(notifications);
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
                    return Task.FromResult(behaviorNotifications.Where(n => notificationNames.Contains(n.Name)));
                }
            }
            
            return Task.FromResult<IEnumerable<EventHolder>>(Array.Empty<EventHolder>());
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}