using System;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.EventHandlers
{
    internal class NotificationsHandler : IActivityEventHandler
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
                var pendingNotifications = await Hub.GetNotificationsAsync(
                    context.Behavior.Id,
                    request.NotificationNames,
                    DateTime.Now - request.Period
                );
                
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
