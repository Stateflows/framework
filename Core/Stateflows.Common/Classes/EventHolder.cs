using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stateflows.Common.Interfaces;

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

        public abstract Task<EventStatus> DoProcessAsync(IStateflowsExecutor executor);

        public abstract Task<EventStatus> ProcessEventAsync(IStateflowsEngine engine, BehaviorId id, List<Exception> exceptions, Dictionary<object, EventHolder> responses);

        public abstract Task<EventStatus> ExecuteBehaviorAsync(IStateflowsProcessor processor, EventStatus result, IStateflowsExecutor stateflowsExecutor);
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

        public override Task<EventStatus> DoProcessAsync(IStateflowsExecutor executor)
            => executor.DoProcessAsync(this);

        public override Task<EventStatus> ProcessEventAsync(IStateflowsEngine engine, BehaviorId id, List<Exception> exceptions, Dictionary<object, EventHolder> responses)
            => engine.ProcessEventAsync(id, this, exceptions, responses);

        public override Task<EventStatus> ExecuteBehaviorAsync(IStateflowsProcessor processor, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => processor.ExecuteBehaviorAsync(this, result, stateflowsExecutor);
    }
}
