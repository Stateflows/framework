using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Reports the status and payload validation result of an event delivery.</summary>
    public class SendResult
    {
        /// <summary>Initializes an empty result for JSON deserialization.</summary>
        [JsonConstructor]
        protected SendResult() { }

        /// <summary>Creates a send result.</summary>
        /// <param name="status">The event delivery status.</param>
        /// <param name="validation">The payload validation result.</param>
        public SendResult(EventStatus status, EventValidation validation)
        {
            Status = status;
            Validation = validation;
        }

        /// <summary>Gets or sets the delivery status.</summary>
        public EventStatus Status { get; set; }
        
        /// <summary>Gets the name of the delivery status.</summary>
        public string StatusText => Enum.GetName(typeof(EventStatus), Status);
        
        /// <summary>Gets or sets the payload validation result.</summary>
        public EventValidation Validation { get; set; }
    }
}
