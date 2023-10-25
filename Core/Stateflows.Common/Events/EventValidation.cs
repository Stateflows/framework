using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stateflows.Common.Extensions;

namespace Stateflows.Common
{
    public sealed class EventValidation
    {
        public EventValidation(bool isValid, IEnumerable<ValidationResult> validationResults)
        {
            IsValid = isValid;
            ValidationResults = validationResults;
        }

        public bool IsValid { get; }

        public IEnumerable<ValidationResult> ValidationResults { get; }
    }
}
