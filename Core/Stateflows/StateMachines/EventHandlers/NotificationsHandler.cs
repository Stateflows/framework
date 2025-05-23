using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

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

        public async Task<EventStatus> TryHandleEventAsync<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is NotificationsRequest request)
            {
                var pendingNotifications = (await Hub.GetNotificationsAsync(context.Behavior.Id))
                    .Where(h => request.NotificationNames.Contains(h.Name))
                    .Where(h => h.SentAt >= DateTime.Now - request.Period)
                    .ToArray();
                
                request.Respond(
                    new NotificationsResponse
                    {
                        Notifications = pendingNotifications
                    });
                
                return EventStatus.Consumed;
            }
            
            return EventStatus.NotConsumed;
        }
    }
}
