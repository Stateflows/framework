using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using System.Threading;

namespace Stateflows.Common.Context
{
    /// <summary>Contains persisted and runtime data associated with a behavior execution.</summary>
    /// <remarks>Storage providers hydrate and dehydrate this object to preserve behavior state and scheduled work.</remarks>
    public class StateflowsContext
    {
        private sealed record ExecutionTriggerFrame(StateflowsContext Context, EventHolder Holder, ExecutionTriggerFrame? Previous);
        private static readonly AsyncLocal<ExecutionTriggerFrame?> executionTrigger = new();
        private EventHolder? executionTriggerHolder;
        private sealed record ExecutionModeFrame(StateflowsContext Context, ExecutionMode Mode, ExecutionModeFrame? Previous);
        private static readonly AsyncLocal<ExecutionModeFrame?> executionMode = new();

        /// <summary>Gets the ownership of the current asynchronous invocation.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public ExecutionMode ExecutionMode
        {
            get
            {
                for (var frame = executionMode.Value; frame != null; frame = frame.Previous)
                    if (ReferenceEquals(frame.Context, this))
                        return frame.Mode;
                return ExecutionMode.Transactional;
            }
        }

        /// <summary>Scopes invocation ownership without changing event delivery metadata.</summary>
        public IDisposable PushExecutionMode(ExecutionMode mode)
        {
            var previous = executionMode.Value;
            executionMode.Value = new ExecutionModeFrame(this, mode, previous);
            return new ExecutionModeScope(previous);
        }

        private sealed class ExecutionModeScope(ExecutionModeFrame? previous) : IDisposable
        {
            private int disposed;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 0)
                    executionMode.Value = previous;
            }
        }

        /// <summary>Creates a context without assigning a behavior identifier.</summary>
        public StateflowsContext()
        {
            Stored = true;
        }

        /// <summary>Creates a context for a behavior that has not yet been persisted.</summary>
        /// <param name="id">The identifier of the behavior represented by the context.</param>
        public StateflowsContext(BehaviorId id)
        {
            Id = id;
            Stored = false;
        }

        /// <summary>Gets or sets the behavior identifier.</summary>
        public BehaviorId Id { get; set; }

        /// <summary>Gets or sets the behavior definition version associated with this context.</summary>
        public int Version { get; set; } = 0;

        /// <summary>Gets or sets whether this context represents a deleted behavior.</summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Deleted { get; set; }

        /// <summary>Gets or sets whether this context is considered persisted.</summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Stored { get; set; }

        /// <summary>Gets or sets the current lifecycle status of the behavior.</summary>
        public BehaviorStatus Status { get; set; } = BehaviorStatus.Unknown;

        /// <summary>Gets or sets the time of the behavior's most recent execution.</summary>
        public DateTime LastExecutedAt { get; set; }

        /// <summary>Determines whether event receipts should be serialized.</summary>
        /// <returns><see langword="true"/> when at least one receipt is present.</returns>
        public bool ShouldSerializeEventReceipts()
            => EventReceipts.Count != 0;

        /// <summary>Gets or sets confirmed event identifiers and their absolute UTC deduplication expiry times.</summary>
        public Dictionary<Guid, DateTime> EventReceipts { get; set; } = [];

        private DateTime? triggerTime;

        /// <summary>Gets or sets the earliest pending time-event trigger in UTC.</summary>
        /// <remarks>Local values are converted to UTC; unspecified values are treated as UTC.</remarks>
        public DateTime? TriggerTime
        {
            get => triggerTime;
            set => triggerTime = Utilities.UtcTime.Normalize(value);
        }

        /// <summary>Gets or sets whether startup-triggered work is pending.</summary>
        public bool TriggerOnStartup { get; set; }

        /// <summary>Determines whether pending time events should be serialized.</summary>
        /// <returns><see langword="true"/> when at least one time event is pending.</returns>
        public bool ShouldSerializePendingTimeEvents()
            => PendingTimeEvents.Any();

        /// <summary>Gets or sets pending time events keyed by their delivery identifiers.</summary>
        public Dictionary<Guid, TimeEvent> PendingTimeEvents { get; set; } = [];

        /// <summary>Determines whether pending startup events should be serialized.</summary>
        /// <returns><see langword="true"/> when at least one startup event is pending.</returns>
        public bool ShouldSerializePendingStartupEvents()
            => PendingStartupEvents.Count != 0;

        /// <summary>Gets or sets pending startup events keyed by their delivery identifiers.</summary>
        public Dictionary<Guid, Startup> PendingStartupEvents { get; set; } = [];

        /// <summary>Gets or sets the event subscriptions made by this behavior, grouped by source behavior.</summary>
        public Dictionary<BehaviorId, List<string>> Subscriptions { get; set; } = [];

        /// <summary>Adds an event-name subscription to a source behavior.</summary>
        /// <param name="subscribeeBehaviorId">The behavior whose events are subscribed to.</param>
        /// <param name="eventName">The event name to receive.</param>
        /// <returns><see langword="true"/> if the subscription was newly added.</returns>
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

        /// <summary>Removes an event-name subscription from a source behavior.</summary>
        /// <param name="subscribeeBehaviorId">The behavior whose events are subscribed to.</param>
        /// <param name="eventName">The event name to stop receiving.</param>
        /// <returns><see langword="true"/> if the subscription existed and was removed.</returns>
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

        /// <summary>Gets or sets subscriber behaviors grouped by notification name.</summary>
        public Dictionary<string, List<BehaviorId>> Subscribers { get; set; } = [];

        /// <summary>Adds a subscriber for each supplied notification name.</summary>
        /// <param name="subscriberBehaviorId">The behavior that subscribes to notifications.</param>
        /// <param name="notificationNames">The notification names to subscribe to.</param>
        /// <returns><see langword="true"/> after processing the supplied names.</returns>
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

        /// <summary>Removes a subscriber from each supplied notification name.</summary>
        /// <param name="subscriberBehaviorId">The behavior whose subscriptions are removed.</param>
        /// <param name="notificationNames">The notification names to unsubscribe from.</param>
        /// <returns><see langword="true"/> after processing the supplied names.</returns>
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

        /// <summary>Determines whether behavior-scoped values should be serialized.</summary>
        /// <returns><see langword="true"/> when at least one value is present.</returns>
        public bool ShouldSerializeValues()
            => Values.Count != 0;

        /// <summary>Gets behavior-scoped values persisted with this context.</summary>
        public Dictionary<string, object> Values { get; } = [];
        
        // public bool ShouldSerializeGlobalValues()
        //     => GlobalValues.Count != 0;

        /// <summary>Gets global values persisted with this context.</summary>
        public Dictionary<string, string> GlobalValues { get; } = [];
        
        /// <summary>Gets or sets the identifier of the behavior that owns this context, when applicable.</summary>
        public BehaviorId? ContextOwnerId { get; set; }
        /// <summary>Gets or sets the identifier of the parent behavior, when applicable.</summary>
        public BehaviorId? ContextParentId { get; set; }
        
        /// <summary>Gets runtime-only metadata that is not persisted.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public Dictionary<string, object> RuntimeMetadata { get; } = [];

        /// <summary>Gets or sets the event envelope that triggered the current execution.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public EventHolder? ExecutionTriggerHolder
        {
            get
            {
                for (var frame = executionTrigger.Value; frame != null; frame = frame.Previous)
                {
                    if (ReferenceEquals(frame.Context, this))
                        return frame.Holder;
                }

                return executionTriggerHolder;
            }
            set => executionTriggerHolder = value;
        }

        /// <summary>Scopes the triggering event to the current asynchronous execution.</summary>
        public IDisposable PushExecutionTriggerHolder(EventHolder holder)
        {
            ArgumentNullException.ThrowIfNull(holder);
            var previous = executionTrigger.Value;
            executionTrigger.Value = new ExecutionTriggerFrame(this, holder, previous);
            return new ExecutionTriggerScope(previous);
        }

        private sealed class ExecutionTriggerScope(ExecutionTriggerFrame? previous) : IDisposable
        {
            private int disposed;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) == 0)
                    executionTrigger.Value = previous;
            }
        }

        /// <summary>Gets exceptions collected during the current execution.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [JsonIgnore]
        public ConcurrentQueue<Exception> ExecutionExceptions { get; } = new();
    }
}
