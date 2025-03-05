using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.EventHandlers
{
    internal class NotificationsHandler : IStateMachineEventHandler
    {
        private INotificationsHub Hub;
        public NotificationsHandler(INotificationsHub hub)
        {
            Hub = hub;
        }
        
        public Type EventType => typeof(NotificationsRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(IEventInspectionContext<TEvent> context)
        {
            if (context.Event is NotificationsRequest request)
            {
                var pendingNotifications = Hub.Notifications.TryGetValue(context.Behavior.Id, out var notifications)
                    ? notifications
                        .Where(h => request.NotificationNames.Contains(h.Name))
                        .Where(h => h.SentAt >= DateTime.Now - request.Period)
                        .ToArray()
                    : Array.Empty<EventHolder>();
                
                request.Respond(
                    new NotificationsResponse
                    {
                        Notifications = pendingNotifications
                    });
                
                return Task.FromResult(EventStatus.Consumed);
            }
            
            return Task.FromResult(EventStatus.NotConsumed);
        }
    }
}
