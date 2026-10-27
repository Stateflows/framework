using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Contains the validity result and validation errors for an event payload.</summary>
    public sealed class EventValidation
    {
        /// <summary>Creates an event validation result.</summary>
        /// <param name="isValid">Whether the event passed validation.</param>
        /// <param name="validationResults">Validation errors, if any.</param>
        public EventValidation(bool isValid, IEnumerable<ValidationResult> validationResults = null)
        {
            IsValid = isValid;
            ValidationResults = validationResults ?? Array.Empty<ValidationResult>();
        }

        /// <summary>Gets whether the event passed validation.</summary>
        public bool IsValid { get; }

        /// <summary>Gets validation errors associated with the event.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<ValidationResult> ValidationResults { get; }
    }
}
