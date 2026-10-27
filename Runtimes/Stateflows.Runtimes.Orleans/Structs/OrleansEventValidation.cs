using System.ComponentModel.DataAnnotations;
using Stateflows.Common;

namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansEventValidation")]
public struct OrleansEventValidation
{
    [Id(0)]
    public bool IsValid { get; set; }

    [Id(1)]
    public List<OrleansValidationResult> ValidationResults { get; set; }
    
    public static implicit operator OrleansEventValidation(EventValidation eventValidation)
        => new OrleansEventValidation
        {
            IsValid = eventValidation.IsValid,
            ValidationResults = eventValidation.ValidationResults.Select(r => (OrleansValidationResult)r).ToList()
        };
    
    public static implicit operator EventValidation(OrleansEventValidation eventValidation)
        => new EventValidation(eventValidation.IsValid, eventValidation.ValidationResults.Select(r => (ValidationResult)r).ToArray());
}