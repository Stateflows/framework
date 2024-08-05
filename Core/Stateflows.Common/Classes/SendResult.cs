using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(EventHolder @event, EventStatus status, EventValidation validation = null)
        {
            Event = @event;
            Status = status;
            Validation = validation ?? new EventValidation(true, Array.Empty<ValidationResult>());
        }

        private EventHolder Event { get; set; }

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; set; }
    }
}
