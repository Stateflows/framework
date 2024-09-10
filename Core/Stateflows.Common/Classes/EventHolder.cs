using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public abstract class EventHolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public abstract string Name { get; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();

        public DateTime SentAt { get; set; }

        public BehaviorId SenderId { get; set; }

        private object boxedPayload;

        [JsonIgnore]
        public object BoxedPayload => boxedPayload ??= GetBoxedPayload();

        protected abstract object GetBoxedPayload();

        [JsonIgnore]
        public Type PayloadType => GetPayloadType();

        protected abstract Type GetPayloadType();
    }

    public sealed class EventHolder<TEvent> : EventHolder
    {
        private string name;

        public override string Name => name ??= Event<TEvent>.Name;

        public TEvent Payload { get; set; } = default;

        protected override object GetBoxedPayload()
            => Payload;

        protected override Type GetPayloadType()
            => typeof(TEvent);

        public override bool Equals(object obj)
            => obj is EventHolder holder && holder.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
