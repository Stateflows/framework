using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(Event @event, EventStatus status, EventValidation validation)
        {
            Event = @event;
            Status = status;
            Validation = validation;
        }

        public Event Event { get; set; }

        public EventStatus Status { get; set; }

        public EventValidation Validation { get; set; }
    }
}
