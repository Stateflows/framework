using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public class SendResult
    {
        [JsonConstructor]
        protected SendResult() { }

        public SendResult(EventStatus status, EventValidation validation = null)
        {
            Status = status;
            Validation = validation ?? new EventValidation(true);
        }

        public EventStatus Status { get; set; }
        
        public string StatusText => Enum.GetName(typeof(EventStatus), Status);
        
        public EventValidation Validation { get; set; }
    }
}
