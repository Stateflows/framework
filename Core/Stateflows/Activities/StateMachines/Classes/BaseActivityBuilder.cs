using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class BaseActivityBuilder
    {
        public List<Type> Notifications { get; } = new List<Type>();

        public Subscribe GetSubscribe(BehaviorId behaviorId)
            => new Subscribe()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };

        public Unsubscribe GetUnsubscribe(BehaviorId behaviorId)
            => new Unsubscribe()
            {
                NotificationNames = Notifications
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };
    }
}
