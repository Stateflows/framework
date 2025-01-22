using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Stateflows.Common;

namespace Stateflows.Extensions.FluentValidation
{
    public class FluentValidator : IStateflowsValidator
    {
        private readonly IServiceProvider serviceProvider;
        public FluentValidator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        public async Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            var validator = serviceProvider.GetService<IValidator<TEvent>>();
            if (validator == null)
            {
                return true;
            }
            
            var result = await validator.ValidateAsync(@event);
            validationResults.AddRange(
                result.Errors.Select(error => new ValidationResult(error.ErrorMessage, new[] { error.PropertyName }))
            );
            
            return result.IsValid;
        }
    }
}