using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(EventHolder eventHolder, EventStatus status, EventValidation validation = null)
        {
            EventHolder = eventHolder;
            Status = status;
            Validation = validation ?? new EventValidation(true, Array.Empty<ValidationResult>());
        }

        private EventHolder EventHolder { get; set; }

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; set; }
    }
}
