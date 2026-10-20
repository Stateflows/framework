using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stateflows.Common.Validators
{
    public class AttributeValidator : IStateflowsValidator
    {
        public Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            object value = @event;
            var validationContext = new ValidationContext(value, serviceProvider: null, items: null);

            return Task.FromResult(Validator.TryValidateObject(value, validationContext, validationResults, true));
        }
    }
}