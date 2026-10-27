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
    /// <summary>Adapts registered FluentValidation validators to the Stateflows validation pipeline.</summary>
    public class FluentValidator : IStateflowsValidator
    {
        private readonly IServiceProvider serviceProvider;
        /// <summary>Creates the adapter that resolves validators from the application service provider.</summary>
        /// <param name="serviceProvider">The provider used to resolve <see cref="IValidator{T}"/> implementations.</param>
        public FluentValidator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        
        /// <summary>Validates an event with its registered FluentValidation validator, if present.</summary>
        /// <typeparam name="TEvent">The declared event type.</typeparam>
        /// <param name="event">The event instance to validate.</param>
        /// <param name="validationResults">Receives validation failures converted to data-annotation results.</param>
        /// <returns><see langword="true"/> when no validator is registered or validation succeeds; otherwise, <see langword="false"/>.</returns>
        /// <remarks>When the event is a structural event and no validator for its declared type exists, the adapter also checks for an <see cref="IValidator{T}"/> registered for <see cref="StructuralEvent"/>.</remarks>
        public async Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            var validator = serviceProvider.GetService<IValidator<TEvent>>();
            if (validator == null)
            {
                if (@event is StructuralEvent structuralEvent && typeof(TEvent) != typeof(StructuralEvent))
                {
                    return await ValidateAsync(structuralEvent, validationResults);
                }
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