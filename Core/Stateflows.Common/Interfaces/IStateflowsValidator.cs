using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IStateflowsValidator
    {
        Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults);
    }
}
