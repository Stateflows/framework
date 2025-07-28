using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.StateMachines.Interfaces
{
    internal class BaseEmbeddedBehaviorBuilder
    {
        public List<Type> Subscriptions { get; } = new List<Type>();
        
        public List<Type> Relays { get; } = new List<Type>();

        public Subscribe GetSubscribe(BehaviorId behaviorId)
            => new Subscribe()
            {
                NotificationNames = Subscriptions
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };

        public Unsubscribe GetUnsubscribe(BehaviorId behaviorId)
            => new Unsubscribe()
            {
                NotificationNames = Subscriptions
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };

        public StartRelay GetStartRelay(BehaviorId behaviorId)
            => new StartRelay()
            {
                NotificationNames = Relays
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };

        public StopRelay GetStopRelay(BehaviorId behaviorId)
            => new StopRelay()
            {
                NotificationNames = Relays
                    .Select(notificationType => notificationType.GetEventName())
                    .ToList(),
                BehaviorId = behaviorId
            };
    }
}
