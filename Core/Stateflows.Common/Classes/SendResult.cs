using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(EventHolder eventHolder, EventStatus status, IEnumerable<EventHolder> notifications = null, EventValidation validation = null)
        {
            EventHolder = eventHolder;
            Status = status;
            Notifications = notifications ?? Array.Empty<EventHolder>();
            Validation = validation ?? new EventValidation(true, Array.Empty<ValidationResult>());
        }

        private EventHolder EventHolder { get; set; }

        public EventStatus Status { get; set; }
        
        public string StatusText => Enum.GetName(typeof(EventStatus), Status);
        
        public IEnumerable<EventHolder> Notifications { get; set; }

        public EventValidation Validation { get; set; }
    }
}
