using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common
{
    /// <summary>Provides the transport envelope and delivery metadata for an event payload.</summary>
    public abstract class EventHolder
    {
        /// <summary>Gets or sets the identifier of this delivery envelope.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gets the event name represented by this envelope.</summary>
        public abstract string Name { get; }

        /// <summary>Gets or sets headers attached to this delivery.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public Dictionary<string, EventHeader> Headers { get; set; } = [];
        
        /// <summary>Gets or sets the delivery time-to-live in seconds.</summary>
        public int TimeToLive { get; set; }
        
        /// <summary>Gets or sets whether this event is retained by the notification store.</summary>
        public bool Retained { get; set; }

        /// <summary>Gets or sets the time at which the event was sent.</summary>
        public DateTime SentAt { get; set; }

        /// <summary>Gets or sets the identifier of the sending behavior, when available.</summary>
        public BehaviorId? SenderId { get; set; }

        /// <summary>Gets or sets the validation result for the current delivery.</summary>
        /// <remarks>The result includes payload validation performed before the event is processed.</remarks>
        [JsonIgnore]
        public EventValidation Validation { get; set; } = new EventValidation(true);

        private object boxedPayload;

        /// <summary>Gets the payload boxed as an object.</summary>
        [JsonIgnore]
        public object BoxedPayload => boxedPayload ??= GetBoxedPayload();

        /// <summary>Returns the payload boxed as an object.</summary>
        /// <returns>The payload value.</returns>
        protected abstract object GetBoxedPayload();

        /// <summary>Gets the CLR type of the event payload.</summary>
        [JsonIgnore]
        public Type PayloadType => GetPayloadType();

        /// <summary>Returns the declared CLR type of the event payload.</summary>
        /// <returns>The payload type.</returns>
        protected abstract Type GetPayloadType();

        /// <summary>Passes this envelope to an executor for delivery.</summary>
        /// <param name="executor">The executor that performs event processing.</param>
        /// <returns>The resulting event status.</returns>
        public abstract Task<EventStatus> ExecuteAsync(IStateflowsExecutor executor);

        /// <summary>Processes the event for a behavior as part of an engine execution.</summary>
        /// <param name="engine">The engine processing the event.</param>
        /// <param name="id">The target behavior identifier.</param>
        /// <param name="exceptions">The collection used to report processing exceptions.</param>
        /// <param name="responses">The request-response envelope map used during processing.</param>
        /// <returns>The resulting event status.</returns>
        public abstract Task<EventStatus> ProcessEventAsync(IStateflowsEngine engine, BehaviorId id, List<Exception> exceptions, Dictionary<object, EventHolder> responses);
        
        /// <summary>Sends this event through a behavior handle.</summary>
        /// <param name="behavior">The behavior receiving the event.</param>
        /// <param name="headers">Optional delivery headers.</param>
        /// <returns>The send result.</returns>
        public abstract Task<SendResult> SendAsync(IBehavior behavior, IDictionary<string, EventHeader> headers = null);

        /// <summary>Delivers this envelope to a typed notification handler.</summary>
        /// <param name="handler">The handler receiving the notification.</param>
        /// <returns>A task that completes when the handler has processed it.</returns>
        public abstract Task NotifyAsync(ITypedNotificationHandler handler);

        /// <summary>Validates the payload with a validator and collects validation results.</summary>
        /// <param name="validator">The validator to run.</param>
        /// <param name="validationResults">The list to which validation errors are added.</param>
        /// <returns><see langword="true"/> when the payload passes validation.</returns>
        protected abstract Task<bool> InternalValidateAsync(IStateflowsValidator validator, List<ValidationResult> validationResults);
        
        /// <summary>Validates the payload using the supplied validators and updates <see cref="Validation"/>.</summary>
        /// <param name="validators">Validators to run against the payload.</param>
        /// <returns>The aggregate validation result.</returns>
        public async Task<EventValidation> ValidateAsync(IStateflowsValidator[] validators)
        {
            var validationResults = new List<ValidationResult>();
            var isValid = true;
            
            if (!PayloadType.IsClass) return Validation = new EventValidation(true, validationResults);
            
            foreach (var validator in validators)
            {
                if (!await InternalValidateAsync(validator, validationResults))
                {
                    isValid = false;
                }
            }

            return Validation = new EventValidation(isValid, validationResults);
        }
    }

    /// <summary>An event envelope that retains the payload's compile-time type.</summary>
    /// <typeparam name="TEvent">The payload type.</typeparam>
    public sealed class EventHolder<TEvent> : EventHolder
    {
        private string name;

        /// <summary>Gets the structural event's declared name, or the standard event name for the payload type.</summary>
        public override string Name => Payload is StructuralEvent structuralEvent
            ? structuralEvent.Name
            : name ??= Event<TEvent>.Name;

        /// <summary>Gets or sets the event payload.</summary>
        public TEvent Payload { get; set; } = default;

        /// <inheritdoc />
        protected override object GetBoxedPayload()
            => Payload;

        /// <inheritdoc />
        protected override Type GetPayloadType()
            => typeof(TEvent);

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is EventHolder holder && holder.Id == Id;

        /// <inheritdoc />
        public override int GetHashCode()
            => Id.GetHashCode();

        /// <inheritdoc />
        [DebuggerHidden]
        public override Task<EventStatus> ExecuteAsync(IStateflowsExecutor executor)
            => executor.DoProcessAsync(this);

        /// <inheritdoc />
        [DebuggerHidden]
        public override Task<EventStatus> ProcessEventAsync(IStateflowsEngine engine, BehaviorId id, List<Exception> exceptions, Dictionary<object, EventHolder> responses)
            => engine.ProcessEventAsync(id, this, exceptions, responses);

        /// <inheritdoc />
        public override Task<SendResult> SendAsync(IBehavior behavior, IDictionary<string, EventHeader> headers = null)
            => behavior.SendAsync(this, headers);

        /// <inheritdoc />
        public override Task NotifyAsync(ITypedNotificationHandler handler)
            => handler.HandleNotificationAsync(this);

        /// <inheritdoc />
        protected override Task<bool> InternalValidateAsync(IStateflowsValidator validator,
            List<ValidationResult> validationResults)
            => validator.ValidateAsync(Payload, validationResults);
    }
}
