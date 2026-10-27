using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Validates an incoming event payload before behavior processing.</summary>
    public interface IStateflowsValidator
    {
        /// <summary>Validates an event and appends any validation failures to the supplied collection.</summary>
        /// <typeparam name="TEvent">The event payload type.</typeparam>
        /// <param name="event">The event payload to validate.</param>
        /// <param name="validationResults">Receives validation errors.</param>
        /// <returns><see langword="true"/> when the event is valid.</returns>
        Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults);
    }
}
