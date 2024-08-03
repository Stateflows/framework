using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(Event @event, EventStatus status, EventValidation validation = null)
        {
            Event = @event;
            Status = status;
            Validation = validation ?? new EventValidation(true, Array.Empty<ValidationResult>());
        }

        public Event Event { get; set; }

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; set; }
    }
}
