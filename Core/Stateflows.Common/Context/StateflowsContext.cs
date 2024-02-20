using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        public BehaviorStatus Status { get; set; } = BehaviorStatus.Unknown;

        public DateTime LastExecutedAt { get; set; }

        public DateTime? TriggerTime { get; set; }

        public bool ShouldSerializePendingTimeEvents()
            => PendingTimeEvents.Any();

        public Dictionary<Guid, TimeEvent> PendingTimeEvents { get; set; } = new Dictionary<Guid, TimeEvent>();

        public Dictionary<BehaviorId, List<string>> Subscriptions { get; set; } = new Dictionary<BehaviorId, List<string>>();

        public void AddSubscription(BehaviorId subscribeeBehaviorId, string eventName)
        {

        }

        public Dictionary<string, List<BehaviorId>> Subscribers { get; set; } = new Dictionary<string, List<BehaviorId>>();

        public bool AddSubscriber(BehaviorId subscriberBehaviorId, string eventName)
        {
            if (!Subscribers.TryGetValue(eventName, out var behaviorIds))
            {
                behaviorIds = new List<BehaviorId>();
                Subscribers[eventName] = behaviorIds;
            }

            if (!behaviorIds.Contains(subscriberBehaviorId))
            {
                behaviorIds.Add(subscriberBehaviorId);

                return true;
            }

            return false;
        }

        public bool RemoveSubscriber(BehaviorId subscriberBehaviorId, string eventName)
        {
            if (
                Subscribers.TryGetValue(eventName, out var behaviorIds) &&
                behaviorIds.Contains(subscriberBehaviorId)
            )
            {
                behaviorIds.Remove(subscriberBehaviorId);

                return true;
            }

            return false;
        }

        public bool ShouldSerializeValues()
            => Values.Any();

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public bool ShouldSerializeGlobalValues()
            => GlobalValues.Any();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
    }
}
