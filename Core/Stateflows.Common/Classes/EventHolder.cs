using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public abstract class EventHolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        protected string name;
        public virtual string Name => name ??= GetType().GetTokenName();

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();

        public DateTime SentAt { get; set; }

        public BehaviorId SenderId { get; set; }

        [JsonIgnore]
        public object BoxedPayload { get; }

        protected abstract object GetBoxedPayload();
    }

    public class EventHolder<TEvent> : EventHolder
    {
        public EventHolder()
        {
            Payload = default;
        }

        public override string Name => name ??= typeof(TEvent).GetReadableName();

        public TEvent Payload { get; set; }

        protected override object GetBoxedPayload()
            => Payload;

        public override bool Equals(object obj)
            => obj is EventHolder holder && holder.Id == Id;

        public override int GetHashCode()
            => Id.GetHashCode();
    }
}
