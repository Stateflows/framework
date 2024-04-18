using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(object @event, EventStatus status, EventValidation validation = null)
        {
            Event = @event;
            Status = status;
            Validation = validation ?? new EventValidation(true, Array.Empty<ValidationResult>());
        }

        public object Event { get; set; }

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; set; }
    }

    public class SendResult<TEvent> : SendResult
    {
        public SendResult(TEvent @event, EventStatus status, EventValidation validation = null) : base(@event, status, validation)
        { }

        public new TEvent Event
        {
            get => (TEvent)base.Event;
            set => base.Event = value;
        }
    }
}
