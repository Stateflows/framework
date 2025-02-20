using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Validators
{
    public class AttributeValidator : IStateflowsValidator
    {
        public Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            var validationContext = new ValidationContext(@event, serviceProvider: null, items: null);

            return Task.FromResult(Validator.TryValidateObject(@event, validationContext, validationResults, true));
        }
    }
}