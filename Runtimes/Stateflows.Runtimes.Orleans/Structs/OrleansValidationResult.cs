using System.ComponentModel.DataAnnotations;
using Stateflows.Common;

namespace Stateflows;

[GenerateSerializer]
[Alias("Stateflows.OrleansValidationResult")]
public struct OrleansValidationResult
{
    [Id(0)]
    public List<string> MemberNames { get; set; }

    [Id(1)]
    public string? ErrorMessage { get; set; }
    
    public static implicit operator OrleansValidationResult(ValidationResult eventValidation)
        => new OrleansValidationResult
        {
            MemberNames = eventValidation.MemberNames.ToList(),
            ErrorMessage = eventValidation.ErrorMessage,
        };
    
    public static implicit operator ValidationResult(OrleansValidationResult eventValidation)
        => new ValidationResult(eventValidation.ErrorMessage, eventValidation.MemberNames.ToArray());
}