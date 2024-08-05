using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Activities.Extensions;
using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class IntegratedActivityBuilder : IIntegratedActivityBuilder
    {
        public List<Type> Notifications { get; } = new List<Type>();

        public IntegratedActivityBuilder(IntegratedActivityBuildAction buildAction)
        {
            buildAction?.Invoke(this);
        }

        public Subscribe GetSubscriptionRequest()
            => new Subscribe()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList()
            };

        public Unsubscribe GetUnsubscriptionRequest()
            => new Unsubscribe()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList()
            };

        public Task SubscribeAsync(IBehavior behavior)
            => Notifications.Any()
                ? behavior.RequestAsync(GetSubscriptionRequest())
                : Task.CompletedTask;

        public Task UnsubscribeAsync(IBehavior behavior)
            => Notifications.Any()
                ? behavior.RequestAsync(GetUnsubscriptionRequest())
                : Task.CompletedTask;

        public IIntegratedActivityBuilder AddSubscription<TNotification>()
            where TNotification : Notification, new()
        {
            Notifications.Add(typeof(TNotification));

            return this;
        }
    }
}
