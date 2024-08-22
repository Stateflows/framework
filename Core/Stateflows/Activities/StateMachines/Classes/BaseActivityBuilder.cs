using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class BaseActivityBuilder
    {
        public List<Type> Notifications { get; } = new List<Type>();

        public SubscriptionRequest GetSubscriptionRequest(BehaviorId behaviorId)
            => new SubscriptionRequest()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };

        public UnsubscriptionRequest GetUnsubscriptionRequest(BehaviorId behaviorId)
            => new UnsubscriptionRequest()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };
    }
}
