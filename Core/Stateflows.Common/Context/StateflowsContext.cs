using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public StateflowsContext()
        {
            Stored = true;
        }

        public StateflowsContext(BehaviorId id)
        {
            Id = id;
            Stored = false;
        }

        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        [JsonIgnore]
        public bool Deleted { get; set; }

        [JsonIgnore]
        public bool Stored { get; set; }

        public BehaviorStatus Status { get; set; } = BehaviorStatus.Unknown;

        public DateTime LastExecutedAt { get; set; }

        public DateTime? TriggerTime { get; set; }

        public bool TriggerOnStartup { get; set; }

        public bool ShouldSerializePendingTimeEvents()
            => PendingTimeEvents.Any();

        public Dictionary<Guid, TimeEvent> PendingTimeEvents { get; set; } = new Dictionary<Guid, TimeEvent>();

        public bool ShouldSerializePendingStartupEvents()
            => PendingStartupEvents.Any();

        public Dictionary<Guid, Startup> PendingStartupEvents { get; set; } = new Dictionary<Guid, Startup>();

        public Dictionary<BehaviorId, List<string>> Subscriptions { get; set; } = new Dictionary<BehaviorId, List<string>>();

        public bool AddSubscription(BehaviorId subscribeeBehaviorId, string eventName)
        {
            if (!Subscriptions.TryGetValue(subscribeeBehaviorId, out var eventNames))
            {
                eventNames = new List<string>();
                Subscriptions[subscribeeBehaviorId] = eventNames;
            }

            if (!eventNames.Contains(eventName))
            {
                eventNames.Add(eventName);

                return true;
            }

            return false;
        }

        public bool RemoveSubscription(BehaviorId subscribeeBehaviorId, string eventName)
        {
            if (
                Subscriptions.TryGetValue(subscribeeBehaviorId, out var eventNames) &&
                eventNames.Contains(eventName)
            )
            {
                eventNames.Remove(eventName);

                return true;
            }

            return false;
        }

        public Dictionary<string, List<BehaviorId>> Subscribers { get; set; } = new Dictionary<string, List<BehaviorId>>();

        public bool AddSubscribers(BehaviorId subscriberBehaviorId, IEnumerable<string> notificationNames)
        {
            foreach (var notificationName in notificationNames)
            {
                if (!Subscribers.TryGetValue(notificationName, out var behaviorIds))
                {
                    behaviorIds = new List<BehaviorId>();
                    Subscribers[notificationName] = behaviorIds;
                }

                if (!behaviorIds.Contains(subscriberBehaviorId))
                {
                    behaviorIds.Add(subscriberBehaviorId);
                }
            }

            return true;
        }

        public bool RemoveSubscribers(BehaviorId subscriberBehaviorId, IEnumerable<string> notificationNames)
        {
            foreach (var notificationName in notificationNames)
            {
                if (
                    Subscribers.TryGetValue(notificationName, out var behaviorIds) &&
                    behaviorIds.Contains(subscriberBehaviorId)
                )
                {
                    behaviorIds.Remove(subscriberBehaviorId);
                }
            }

            return true;
        }

        public Dictionary<string, List<BehaviorId>> Relays { get; set; } = new Dictionary<string, List<BehaviorId>>();

        public bool AddRelays(BehaviorId subscriberBehaviorId, IEnumerable<string> notificationNames)
        {
            foreach (var notificationName in notificationNames)
            {
                if (!Relays.TryGetValue(notificationName, out var behaviorIds))
                {
                    behaviorIds = new List<BehaviorId>();
                    Relays[notificationName] = behaviorIds;
                }

                if (!behaviorIds.Contains(subscriberBehaviorId))
                {
                    behaviorIds.Add(subscriberBehaviorId);
                }
            }

            return true;
        }

        public bool RemoveRelays(BehaviorId subscriberBehaviorId, IEnumerable<string> notificationNames)
        {
            foreach (var notificationName in notificationNames)
            {
                if (
                    Relays.TryGetValue(notificationName, out var behaviorIds) &&
                    behaviorIds.Contains(subscriberBehaviorId)
                )
                {
                    behaviorIds.Remove(subscriberBehaviorId);
                }
            }

            return true;
        }

        public bool ShouldSerializeValues()
            => Values.Any();

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public bool ShouldSerializeGlobalValues()
            => GlobalValues.Any();

        public Dictionary<string, string> GlobalValues { get; } = new Dictionary<string, string>();
        
        public BehaviorId ContextOwner { get; set; }
    }
}
