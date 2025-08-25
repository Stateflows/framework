using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    public abstract class EventHolder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public abstract string Name { get; }

        [Newtonsoft.Json.JsonProperty(TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None)]
        public List<EventHeader> Headers { get; set; } = new List<EventHeader>();
        
        public int TimeToLive { get; set; }
        
        public bool Retained { get; set; }

        public DateTime SentAt { get; set; }

        public BehaviorId? SenderId { get; set; }

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

        protected abstract Task<bool> InternalValidateAsync(IStateflowsValidator validator, List<ValidationResult> validationResults);
        
        public async Task<EventValidation> ValidateAsync(IStateflowsValidator[] validators)
        {
            var validationResults = new List<ValidationResult>();
            var isValid = true;

            if (this is EventHolder<CompoundRequestBuilderRequest> compoundRequestHolder)
            {
                var compoundRequest = compoundRequestHolder.Payload;
                var results = new List<RequestResult>();
                foreach (var ev in compoundRequest.Events)
                {
                    var validation = await ev.ValidateAsync(validators);
                    var status = validation.IsValid
                        ? EventStatus.Omitted
                        : EventStatus.Invalid;

                    if (!validation.IsValid)
                    {
                        isValid = false;
                    }

                    results.Add(new RequestResult(
                        null,
                        status,
                        validation
                    ));
                }

                if (!isValid)
                {
                    compoundRequest.Respond(new CompoundResponse() { Results = results });
                }
            }
            else
            {
                if (!PayloadType.IsClass) return new EventValidation(true, validationResults);
                
                foreach (var validator in validators)
                {
                    if (!await InternalValidateAsync(validator, validationResults))
                    {
                        isValid = false;
                    }
                }
            }

            return new EventValidation(isValid, validationResults);
        }
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

        [DebuggerHidden]
        public override Task<EventStatus> DoProcessAsync(IStateflowsExecutor executor)
            => executor.DoProcessAsync(this);

        [DebuggerHidden]
        public override Task<EventStatus> ProcessEventAsync(IStateflowsEngine engine, BehaviorId id, List<Exception> exceptions, Dictionary<object, EventHolder> responses)
            => engine.ProcessEventAsync(id, this, exceptions, responses);

        [DebuggerHidden]
        public override Task<EventStatus> ExecuteBehaviorAsync(IStateflowsProcessor processor, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => processor.ExecuteBehaviorAsync(this, result, stateflowsExecutor);

        protected override Task<bool> InternalValidateAsync(IStateflowsValidator validator,
            List<ValidationResult> validationResults)
            => validator.ValidateAsync(Payload, validationResults);
    }
}
