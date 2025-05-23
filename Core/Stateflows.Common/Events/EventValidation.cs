using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public sealed class EventValidation
    {
        public EventValidation(bool isValid, IEnumerable<ValidationResult> validationResults = null)
        {
            IsValid = isValid;
            ValidationResults = validationResults ?? Array.Empty<ValidationResult>();
        }

        public bool IsValid { get; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<ValidationResult> ValidationResults { get; }
    }
}
